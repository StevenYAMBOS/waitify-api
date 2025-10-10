## Table des matières

- [Vue d'ensemble](#vue-densemble)
- [Architecture](#architecture)
- [Fonctionnement technique](#fonctionnement-technique)
- [Implémentation PostgreSQL](#implémentation-postgresql)
- [Implémentation Golang](#implémentation-golang)
- [Flux de données](#flux-de-données)
- [Gestion des états](#gestion-des-états)
- [Calculs automatiques](#calculs-automatiques)
- [Cas d'usage](#cas-dusage)
- [Optimisations](#optimisations)
- [Troubleshooting](#troubleshooting)

---

## Vue d'ensemble

Le système de file d'attente virtuelle de Waitify repose sur une **architecture hybride PostgreSQL + Golang** pour garantir l'intégrité des données et la performance des opérations en temps réel.

### Principe de fonctionnement

```
Client scanne QR Code
      ↓
Formulaire web (nom + téléphone)
      ↓
POST /queue/join
      ↓
┌─────────────────────────────────┐
│ Validations Golang              │
│ - Business actif                │
│ - File non pleine               │
│ - Pas de doublon                │
└─────────────────────────────────┘
      ↓
┌─────────────────────────────────┐
│ INSERT dans queue_entries       │
└─────────────────────────────────┘
      ↓
┌─────────────────────────────────┐
│ Trigger PostgreSQL              │
│ Recalcul automatique positions  │
└─────────────────────────────────┘
      ↓
SMS confirmation + Position finale
```

---

## Architecture

### Répartition des responsabilités

| Composant | Responsabilités | Pourquoi |
|-----------|----------------|----------|
| **PostgreSQL Triggers** | • Recalcul automatique des positions<br>• Cohérence transactionnelle<br>• Réorganisation après sortie | • Atomicité garantie<br>• Performance native<br>• Impossible d'oublier |
| **Golang Handlers** | • Validation métier<br>• Calcul temps d'attente<br>• Gestion erreurs<br>• Envoi SMS | • Logique complexe<br>• Intégrations externes<br>• Messages utilisateur clairs |

### Schéma de la table `queue_entries`

```sql
queue_entries
├── id (UUID, PK)
├── BusinessId (UUID, FK → businesses)
├── phone (VARCHAR)
├── client_name (VARCHAR)
├── position (INTEGER) ← Recalculé automatiquement
├── estimated_wait_time (INTEGER) ← Calculé par Golang
├── status (VARCHAR) ← waiting | called | served | missed | cancelled
├── called_at (TIMESTAMP)
├── served_at (TIMESTAMP)
├── actual_service_time (INTEGER)
├── sms_sent_count (INTEGER)
├── last_sms_sent_at (TIMESTAMP)
├── created_at (TIMESTAMP)
└── updated_at (TIMESTAMP)
```

---

## Fonctionnement technique

### 1. Gestion des positions

Les positions sont **entièrement automatisées** via des triggers PostgreSQL. Cela garantit que :

- ✅ Les positions sont **toujours cohérentes**
- ✅ Aucune race condition possible (concurrence)
- ✅ Pas besoin de logique manuelle dans le code applicatif

#### Règles de calcul

```
Position = ROW_NUMBER() OVER (ORDER BY created_at ASC)
```

**Filtres appliqués :**

- Même `BusinessId`
- Status = `waiting` uniquement

**Exemples :**

| Client | created_at | Status | Position |
|--------|-----------|--------|----------|
| Alice | 10:00:00 | waiting | 1 |
| Bob | 10:01:30 | waiting | 2 |
| Charlie | 10:02:15 | waiting | 3 |
| David | 10:03:00 | served | ❌ (exclu) |

Si Bob est servi → Trigger recalcule :

| Client | Status | Nouvelle position |
|--------|--------|-------------------|
| Alice | waiting | 1 (inchangé) |
| Bob | served | ❌ (retiré) |
| Charlie | waiting | 2 (était 3) |

---

### 2. Calcul du temps d'attente

Le temps d'attente estimé est calculé **par Golang** au moment de l'insertion :

```go
estimatedWaitMinutes = (currentQueueSize * averageServiceTime) / 60
```

**Variables :**

- `currentQueueSize` : Nombre de clients avec `status = 'waiting'` devant le nouveau client
- `averageServiceTime` : Temps moyen en secondes (colonne `businesses.average_service_time`)

**Exemple :**

```
Business : Boulangerie (average_service_time = 120 secondes)
File actuelle : 3 clients en attente

Nouveau client :
→ Position = 4
→ Temps estimé = (3 × 120) / 60 = 6 minutes
```

---

## Implémentation PostgreSQL

### Triggers automatiques

#### 1. Recalcul des positions

```sql
CREATE OR REPLACE FUNCTION recalculate_queue_positions()
RETURNS TRIGGER AS $$
BEGIN
    -- Recalcule les positions pour le business concerné
    UPDATE queue_entries
    SET position = subquery.new_position
    FROM (
        SELECT 
            id, 
            ROW_NUMBER() OVER (ORDER BY created_at ASC) as new_position
        FROM queue_entries
        WHERE BusinessId = COALESCE(NEW.BusinessId, OLD.BusinessId)
          AND status = 'waiting'
    ) AS subquery
    WHERE queue_entries.id = subquery.id;

    RETURN COALESCE(NEW, OLD);
END;
$$ LANGUAGE plpgsql;
```

#### 2. Application des triggers

```sql
-- Trigger à l'insertion (nouveau client)
CREATE TRIGGER recalculate_positions_on_insert
    AFTER INSERT ON queue_entries
    FOR EACH ROW
    EXECUTE FUNCTION recalculate_queue_positions();

-- Trigger au changement de status (served, missed, cancelled)
CREATE TRIGGER recalculate_positions_on_status_update
    AFTER UPDATE OF status ON queue_entries
    FOR EACH ROW
    WHEN (OLD.status IS DISTINCT FROM NEW.status)
    EXECUTE FUNCTION recalculate_queue_positions();

-- Trigger à la suppression (rare, mais sécurise)
CREATE TRIGGER recalculate_positions_on_delete
    AFTER DELETE ON queue_entries
    FOR EACH ROW
    EXECUTE FUNCTION recalculate_queue_positions();
```

### Index recommandés

```sql
-- Performances pour les requêtes fréquentes
CREATE INDEX idx_queue_entries_business_status 
    ON queue_entries(BusinessId, status);

CREATE INDEX idx_queue_entries_active_position 
    ON queue_entries(BusinessId, position) 
    WHERE status = 'waiting';

CREATE INDEX idx_queue_entries_waiting_by_business 
    ON queue_entries(BusinessId, position, created_at) 
    WHERE status = 'waiting';
```

---

## Implémentation Golang

### Handler : Rejoindre la file

```go
func JoinQueueHandler(w http.ResponseWriter, r *http.Request) {
    // 1. Parse et validation de base
    var req models.JoinQueueRequest
    if err := json.NewDecoder(r.Body).Decode(&req); err != nil {
        http.Error(w, `Corps invalide`, http.StatusBadRequest)
        return
    }

    // 2. Récupérer les paramètres du business
    var business struct {
        IsQueueActive      bool
        MaxQueueSize       int
        AverageServiceTime int
    }
    
    err := database.DB.QueryRow(`
        SELECT is_queue_active, max_queue_size, average_service_time
        FROM businesses
        WHERE id = $1 AND is_active = true
    `, req.BusinessID).Scan(
        &business.IsQueueActive,
        &business.MaxQueueSize,
        &business.AverageServiceTime,
    )

    // 3. Validations métier
    if !business.IsQueueActive {
        http.Error(w, `File fermée`, http.StatusForbidden)
        return
    }

    // 4. Vérifier doublon
    var alreadyInQueue bool
    database.DB.QueryRow(`
        SELECT EXISTS(
            SELECT 1 FROM queue_entries
            WHERE BusinessId = $1 AND phone = $2 AND status = 'waiting'
        )
    `, req.BusinessID, req.Phone).Scan(&alreadyInQueue)

    if alreadyInQueue {
        http.Error(w, `Déjà dans la file`, http.StatusConflict)
        return
    }

    // 5. Vérifier capacité
    var currentQueueSize int
    database.DB.QueryRow(`
        SELECT COUNT(*) FROM queue_entries
        WHERE BusinessId = $1 AND status = 'waiting'
    `, req.BusinessID).Scan(&currentQueueSize)

    if currentQueueSize >= business.MaxQueueSize {
        http.Error(w, `File complète`, http.StatusServiceUnavailable)
        return
    }

    // 6. Calculer temps d'attente
    nextPosition := currentQueueSize + 1
    estimatedWaitMinutes := (currentQueueSize * business.AverageServiceTime) / 60

    // 7. Insertion (le trigger recalculera automatiquement)
    entryID := uuid.New()
    _, err = database.DB.Exec(`
        INSERT INTO queue_entries (
            id, BusinessId, phone, client_name, position, 
            estimated_wait_time, status, created_at, updated_at
        ) VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9)
    `,
        entryID,
        req.BusinessID,
        req.Phone,
        req.ClientName,
        nextPosition,
        estimatedWaitMinutes,
        "waiting",
        time.Now(),
        time.Now(),
    )

    // 8. TODO: Envoyer SMS de confirmation
    // sendSMS(req.Phone, fmt.Sprintf("Position %d, ~%d min", nextPosition, estimatedWaitMinutes))

    // 9. Réponse
    json.NewEncoder(w).Encode(models.JoinQueueResponse{
        Message: "Ajouté à la file",
        Entry: models.QueueEntry{
            ID:                entryID,
            Position:          nextPosition,
            EstimatedWaitTime: estimatedWaitMinutes,
            Status:            "waiting",
        },
    })
}
```

### Modèles de données

```go
// internal/models/queueModels.go
package models

import (
    "time"
    "github.com/google/uuid"
)

type JoinQueueRequest struct {
    BusinessID uuid.UUID `json:"business_id" validate:"required"`
    Phone      string    `json:"phone" validate:"required,e164"`
    ClientName string    `json:"client_name" validate:"required,min=2"`
}

type JoinQueueResponse struct {
    Message string     `json:"message"`
    Entry   QueueEntry `json:"entry"`
}

type QueueEntry struct {
    ID                uuid.UUID `json:"id"`
    BusinessID        uuid.UUID `json:"business_id"`
    Phone             string    `json:"phone"`
    ClientName        string    `json:"client_name"`
    Position          int       `json:"position"`
    EstimatedWaitTime int       `json:"estimated_wait_time"` // minutes
    Status            string    `json:"status"`
    CreatedAt         time.Time `json:"created_at"`
}
```

---

## Flux de données

### Scénario complet : 3 clients

```
T=0   : File vide pour "Boulangerie Martin" (average_service_time = 120s)

T=10:00:00
├─ Alice scanne QR Code
├─ POST /queue/join { phone: "+33612345678", name: "Alice" }
├─ Golang calcule : position = 1, temps = 0 min
├─ INSERT queue_entries
└─ Trigger recalcule : Alice position = 1 ✅

T=10:01:30
├─ Bob scanne QR Code
├─ POST /queue/join { phone: "+33698765432", name: "Bob" }
├─ Golang calcule : position = 2, temps = 2 min
├─ INSERT queue_entries
└─ Trigger recalcule : Alice=1, Bob=2 ✅

T=10:02:15
├─ Charlie scanne QR Code
├─ POST /queue/join { phone: "+33687654321", name: "Charlie" }
├─ Golang calcule : position = 3, temps = 4 min
├─ INSERT queue_entries
└─ Trigger recalcule : Alice=1, Bob=2, Charlie=3 ✅

T=10:05:00
├─ Commerçant appelle Alice
├─ PUT /queue/{alice_id}/status { status: "called" }
├─ SMS envoyé à Alice : "C'est votre tour !"
└─ Trigger recalcule : Bob=1, Charlie=2 ✅

T=10:07:00
├─ Commerçant confirme service Alice
├─ PUT /queue/{alice_id}/status { status: "served" }
└─ Trigger recalcule : Bob=1, Charlie=2 (inchangé)

État final :
| Client  | Status  | Position |
|---------|---------|----------|
| Alice   | served  | ❌       |
| Bob     | waiting | 1        |
| Charlie | waiting | 2        |
```

---

## Gestion des états

### Machine à états

```
┌─────────┐
│ waiting │ ← État initial
└────┬────┘
     │
     ├──→ called   (commerçant appelle)
     │       ├──→ served   (client présent)
     │       └──→ missed   (timeout 5 min)
     │
     └──→ cancelled (client annule)
```

### Transitions autorisées

| De | Vers | Action | Trigger recalcul |
|----|------|--------|------------------|
| waiting | called | Commerçant appelle | ✅ Oui |
| called | served | Client servi | ✅ Oui |
| called | missed | Timeout 5 min | ✅ Oui |
| waiting | cancelled | Client annule | ✅ Oui |

### États finaux (ne recalculent plus)

- `served` : Client a été servi avec succès
- `missed` : Client absent lors de son appel
- `cancelled` : Client a annulé sa place

---

## Calculs automatiques

### 1. Position

**Formule PostgreSQL :**

```sql
ROW_NUMBER() OVER (
    PARTITION BY BusinessId 
    ORDER BY created_at ASC
) WHERE status = 'waiting'
```

**Propriétés :**

- Basée sur `created_at` (FIFO strict)
- Recalculée après chaque changement
- Ignore les status finaux (served, missed, cancelled)

### 2. Temps d'attente estimé

**Formule Golang :**

```go
estimatedWaitMinutes = (clientsAhead * averageServiceTime) / 60
```

**Variables :**

- `clientsAhead` : Nombre de clients avec `status='waiting'` ET `created_at < current_client.created_at`
- `averageServiceTime` : Depuis `businesses.average_service_time` (en secondes)

**Exemple :**

```
Coiffeur : average_service_time = 2700s (45 min)
File actuelle : 2 clients

Nouveau client :
→ Position = 3
→ Temps estimé = (2 × 2700) / 60 = 90 minutes
```

### 3. Mise à jour dynamique

⚠️ **Important** : Le temps d'attente est calculé **une seule fois à l'insertion**. Pour une mise à jour en temps réel :

```go
// À implémenter dans GET /queue/status/:id
func GetQueueStatusHandler(w http.ResponseWriter, r *http.Request) {
    entryID := chi.URLParam(r, "id")
    
    var entry struct {
        Position       int
        BusinessID     uuid.UUID
        OriginalEstimate int
    }
    
    // Récupérer l'entrée
    database.DB.QueryRow(`
        SELECT position, BusinessId, estimated_wait_time
        FROM queue_entries
        WHERE id = $1 AND status = 'waiting'
    `, entryID).Scan(&entry.Position, &entry.BusinessID, &entry.OriginalEstimate)
    
    // Recalculer en temps réel
    var avgServiceTime int
    database.DB.QueryRow(`
        SELECT average_service_time FROM businesses WHERE id = $1
    `, entry.BusinessID).Scan(&avgServiceTime)
    
    currentEstimate := ((entry.Position - 1) * avgServiceTime) / 60
    
    json.NewEncoder(w).Encode(map[string]interface{}{
        "position": entry.Position,
        "estimated_wait_minutes": currentEstimate,
    })
}
```

---

## Cas d'usage

### 1. Client rejoint la file

**Endpoint :** `POST /queue/join`

**Validations Golang :**

- ✅ Business existe et est actif
- ✅ File d'attente ouverte (`is_queue_active = true`)
- ✅ File non pleine (`COUNT(*) < max_queue_size`)
- ✅ Client pas déjà inscrit (même phone + BusinessId + status='waiting')
- ✅ Format téléphone valide

**Actions :**

1. Calcul position initiale : `currentQueueSize + 1`
2. Calcul temps d'attente : `(currentQueueSize * avgServiceTime) / 60`
3. INSERT dans `queue_entries`
4. Trigger PostgreSQL recalcule les positions
5. Envoi SMS confirmation

### 2. Commerçant appelle le client suivant

**Endpoint :** `POST /businesses/:id/queue/next`

```go
func CallNextClientHandler(w http.ResponseWriter, r *http.Request) {
    businessID := chi.URLParam(r, "id")
    
    // Récupérer le premier client en attente
    var nextClient struct {
        ID    uuid.UUID
        Phone string
        Name  string
    }
    
    err := database.DB.QueryRow(`
        SELECT id, phone, client_name
        FROM queue_entries
        WHERE BusinessId = $1 AND status = 'waiting'
        ORDER BY position ASC
        LIMIT 1
    `, businessID).Scan(&nextClient.ID, &nextClient.Phone, &nextClient.Name)
    
    if err == sql.ErrNoRows {
        http.Error(w, `Aucun client en attente`, http.StatusNotFound)
        return
    }
    
    // Mettre à jour le status
    _, err = database.DB.Exec(`
        UPDATE queue_entries
        SET status = 'called', called_at = NOW(), updated_at = NOW()
        WHERE id = $1
    `, nextClient.ID)
    
    // Trigger recalculera automatiquement les positions restantes
    
    // Envoyer SMS "C'est votre tour !"
    // sendSMS(nextClient.Phone, "C'est votre tour chez ...")
    
    json.NewEncoder(w).Encode(map[string]interface{}{
        "message": "Client appelé",
        "client": nextClient,
    })
}
```

### 3. Client annule sa place

**Endpoint :** `DELETE /queue/cancel/:entryId`

```go
func CancelQueueEntryHandler(w http.ResponseWriter, r *http.Request) {
    entryID := chi.URLParam(r, "entryId")
    
    // Vérifier que l'entrée existe et est en attente
    var exists bool
    database.DB.QueryRow(`
        SELECT EXISTS(
            SELECT 1 FROM queue_entries
            WHERE id = $1 AND status = 'waiting'
        )
    `, entryID).Scan(&exists)
    
    if !exists {
        http.Error(w, `Entrée introuvable ou déjà traitée`, http.StatusNotFound)
        return
    }
    
    // Annuler
    _, err := database.DB.Exec(`
        UPDATE queue_entries
        SET status = 'cancelled', updated_at = NOW()
        WHERE id = $1
    `, entryID)
    
    // Trigger recalcule automatiquement les positions restantes
    
    w.WriteHeader(http.StatusNoContent)
}
```

### 4. Timeout automatique (missed)

**Implémentation recommandée :** Job CRON ou background worker

```go
// À exécuter toutes les minutes
func CheckTimeouts() {
    // Récupérer les clients appelés il y a plus de 5 minutes
    rows, err := database.DB.Query(`
        SELECT id, phone, client_name
        FROM queue_entries
        WHERE status = 'called'
          AND called_at < NOW() - INTERVAL '5 minutes'
    `)
    
    for rows.Next() {
        var client struct {
            ID    uuid.UUID
            Phone string
            Name  string
        }
        rows.Scan(&client.ID, &client.Phone, &client.Name)
        
        // Marquer comme manqué
        database.DB.Exec(`
            UPDATE queue_entries
            SET status = 'missed', updated_at = NOW()
            WHERE id = $1
        `, client.ID)
        
        // Trigger recalcule automatiquement
        
        // Envoyer SMS "Vous avez manqué votre tour"
        // sendSMS(client.Phone, "Votre tour est passé. Rescannez le QR code.")
    }
}
```

---

## Optimisations

### 1. Index partiels

```sql
-- Accélère les requêtes sur les clients en attente uniquement
CREATE INDEX idx_waiting_only 
    ON queue_entries(BusinessId, position) 
    WHERE status = 'waiting';
```

### 2. Matérialized view pour statistiques

```sql
-- Pour des tableaux de bord rapides
CREATE MATERIALIZED VIEW queue_stats AS
SELECT 
    BusinessId,
    COUNT(*) FILTER (WHERE status = 'waiting') as waiting_count,
    AVG(estimated_wait_time) FILTER (WHERE status = 'waiting') as avg_wait,
    MAX(position) as max_position
FROM queue_entries
GROUP BY BusinessId;

-- Rafraîchir toutes les 5 minutes
REFRESH MATERIALIZED VIEW queue_stats;
```

### 3. Cache Redis pour `average_service_time`

```go
// Évite de requêter PostgreSQL à chaque inscription
func GetAverageServiceTime(businessID uuid.UUID) int {
    // Vérifier Redis d'abord
    cacheKey := fmt.Sprintf("business:%s:avg_service_time", businessID)
    if cached, err := redisClient.Get(ctx, cacheKey).Int(); err == nil {
        return cached
    }
    
    // Sinon, requêter PostgreSQL
    var avgTime int
    database.DB.QueryRow(`
        SELECT average_service_time FROM businesses WHERE id = $1
    `, businessID).Scan(&avgTime)
    
    // Mettre en cache 1 heure
    redisClient.Set(ctx, cacheKey, avgTime, time.Hour)
    
    return avgTime
}
```

---

## Troubleshooting

### Problème : Positions dupliquées

**Symptôme :**

```sql
SELECT * FROM queue_entries WHERE BusinessId = '...' AND status = 'waiting';
-- Résultat : position 1, 1, 2, 3
```

**Cause :** Le trigger n'a pas été exécuté correctement.

**Solution :**

```sql
-- Recalculer manuellement
UPDATE queue_entries
SET position = subquery.new_position
FROM (
    SELECT 
        id, 
        ROW_NUMBER() OVER (ORDER BY created_at ASC) as new_position
    FROM queue_entries
    WHERE BusinessId = 'problematic-business-id'
      AND status = 'waiting'
) AS subquery
WHERE queue_entries.id = subquery.id;
```

### Problème : Client bloqué en status 'called'

**Symptôme :** Un client reste en `status = 'called'` indéfiniment.

**Solution :** Implémenter le job CRON de timeout.

```go
// À exécuter toutes les minutes
func AutoTimeoutClients() {
    database.DB.Exec(`
        UPDATE queue_entries
        SET status = 'missed', updated_at = NOW()
        WHERE status = 'called'
          AND called_at < NOW() - INTERVAL '5 minutes'
    `)
}
```

### Problème : Temps d'attente incorrect

**Symptôme :** Le temps affiché ne correspond pas à la réalité.

**Cause :** `average_service_time` du business non à jour.

**Solution :**

```sql
-- Mettre à jour avec les données réelles
UPDATE businesses
SET average_service_time = (
    SELECT AVG(actual_service_time)
    FROM queue_entries
    WHERE BusinessId = businesses.id
      AND status = 'served'
      AND actual_service_time IS NOT NULL
)
WHERE id = 'business-id';
```

---

## Prochaines étapes

- [ ] Implémenter l'intégration SMS (Twilio/Vonage)
- [ ] Créer le job CRON de timeout
- [ ] Ajouter WebSocket pour notifications temps réel
- [ ] Implémenter `GET /queue/status/:id` pour suivi en temps réel
- [ ] Créer dashboard commerçant avec statistiques
- [ ] Ajouter tests unitaires pour les triggers PostgreSQL

---

**Auteur :** Steven YAMBOS  
**Dernière mise à jour :** 10 octobre 2025  
**Version :** 1.0.0
