# Waitify — Spécification technique : Analytics commerçant

**Version :** 1.0.0
**Projet :** Waitify — SaaS de gestion de files d'attente virtuelles
**Dépendances :** `DOCUMENTATION_TECHNIQUE.md`, `QUEUES.md`, `Design System v1.0.0`

---

## Table des matières

1. [Contexte & objectifs](#1-contexte--objectifs)
2. [Données disponibles](#2-données-disponibles)
3. [Métriques exposées aux commerçants](#3-métriques-exposées-aux-commerçants)
4. [Fonctionnalités analytics par plan](#4-fonctionnalités-analytics-par-plan)
5. [Architecture technique](#5-architecture-technique)
6. [Endpoints API](#6-endpoints-api)
7. [Schéma de base de données](#7-schéma-de-base-de-données)
8. [Calculs & formules](#8-calculs--formules)
9. [Règles UX & Design System](#9-règles-ux--design-system)

---

## 1. Contexte & objectifs

### Problématique

Les commerçants de proximité n'ont aujourd'hui aucun outil pour mesurer l'impact de leur file d'attente sur leur activité. Ils ne savent pas :

- Quels jours ou heures sont les plus chargés
- Combien de clients ils perdent (annulations, absences)
- Si leur temps de service s'améliore dans le temps
- Quel ROI ils tirent de Waitify

### Objectifs des analytics

- **Donner de la valeur perçue immédiate** : un commerçant qui voit ses données reste abonné
- **Aider à l'optimisation opérationnelle** : ajuster les horaires, le staffing, le `AverageServiceTime`
- **Créer un levier de rétention et d'upsell** : les analytics avancés sont réservés aux plans Pro/Enterprise

### Philosophie (alignée avec le Design System)

Les analytics doivent respecter les trois piliers Waitify :

| Pilier | Traduction dans les analytics |
|---|---|
| **Clarté** | 1 chiffre clé par carte, pas de graphes surchargés |
| **Confiance** | Données toujours à jour, horodatage visible |
| **Efficacité** | Réponse immédiate à "comment s'est passée ma journée ?" |

---

## 2. Données disponibles

Toutes les métriques analytics s'appuient sur les tables existantes. Aucune nouvelle table de collecte n'est nécessaire pour les métriques de base.

### Sources de données

| Table | Données exploitables |
|---|---|
| `QueueEntries` | `Status`, `CreatedAt`, `CalledAt`, `ServedAt`, `ActualServiceTime`, `EstimatedWaitTime` |
| `AnalyticsDaily` | Métriques quotidiennes précalculées (table déjà présente) |
| `SmsLogs` | Taux de livraison SMS, types envoyés, coûts |
| `Businesses` | `AverageServiceTime`, `MaxQueueSize`, `OpeningHours` |
| `Billings` | Consommation SMS par établissement |

### États `QueueEntries` utilisables

```
waiting   → inscriptions totales du jour
called    → tentatives de service
served    → clients effectivement servis ✅
missed    → clients absents à l'appel ❌
cancelled → clients ayant abandonné ❌
```

---

## 3. Métriques exposées aux commerçants

### 3.1 KPIs du jour (vue temps réel)

Affichés en haut du dashboard, mis à jour toutes les minutes.

| Métrique | Source | Calcul |
|---|---|---|
| Clients en attente | `QueueEntries` | `COUNT WHERE Status='waiting' AND BusinessId=X` |
| Clients servis aujourd'hui | `QueueEntries` | `COUNT WHERE Status='served' AND DATE(ServedAt)=today` |
| Temps d'attente moyen actuel | `QueueEntries` | `AVG(EstimatedWaitTime) WHERE Status='waiting'` |
| File ouverte depuis | `Businesses` | Calculé à partir de `UpdatedAt` quand `IsQueueActive` est passé à `true` |

### 3.2 Résumé quotidien

Calculé une fois par jour à minuit et stocké dans `AnalyticsDaily`. Colonnes déjà présentes dans le schéma.

| Métrique | Colonne `AnalyticsDaily` | Description |
|---|---|---|
| Inscriptions totales | `TotalClientsRegistered` | Tous les clients ayant rejoint la file |
| Clients servis | `TotalClientsServed` | Statut `served` |
| Clients manqués | `TotalClientsMissed` | Statut `missed` (timeout 5 min) |
| Clients annulés | `TotalClientsCancelled` | Statut `cancelled` |
| Taux de service | — | `TotalClientsServed / TotalClientsRegistered × 100` |
| Taux d'abandon | `AbandonmentRate` | `(Missed + Cancelled) / Registered × 100` |
| Attente moyenne | `AverageWaitTime` | En minutes |
| Service moyen | `AverageServiceTime` | En secondes |
| Heure de pointe | `PeakHour` | Heure (0–23) avec le plus d'inscriptions |
| Pic de file | `PeakQueueSize` | Nombre maximum de clients simultanés |
| SMS envoyés | `SmsSentCount` | Via `SmsLogs` |

### 3.3 Tendances hebdomadaires et mensuelles

Agrégation des lignes `AnalyticsDaily` sur 7 ou 30 jours.

| Métrique | Calcul |
|---|---|
| Évolution clients servis | `SUM(TotalClientsServed)` par jour sur la période |
| Taux d'abandon moyen | `AVG(AbandonmentRate)` |
| Meilleur jour de la semaine | `GROUP BY EXTRACT(DOW FROM Date)` |
| Évolution du temps de service | `AVG(AverageServiceTime)` par semaine |
| Heures de pointe récurrentes | `MODE() WITHIN GROUP (ORDER BY PeakHour)` |
| Total SMS consommés (facturation) | `SUM(SmsSentCount)` |

### 3.4 Métriques avancées (Pro / Enterprise uniquement)

| Métrique | Description | Calcul |
|---|---|---|
| **Taux de fidélité client** | Clients ayant utilisé la file plusieurs fois (même téléphone) | `COUNT DISTINCT Phone WHERE COUNT > 1` sur `QueueEntries` |
| **Manque à gagner estimé** | Revenu potentiel perdu sur les clients manqués/annulés | `(Missed + Cancelled) × AverageServiceTime × tarif_horaire_estimé` — stocké dans `RevenuePotentialLost` |
| **Comparaison multi-établissements** | Vue consolidée pour les plans Pro/Enterprise | Agrégation par `OwnerId` sur plusieurs `BusinessId` |
| **Temps de réponse commerçant** | Délai moyen entre fin d'un service et appel du suivant | `AVG(CalledAt[n] - ServedAt[n-1])` par `BusinessId` par jour |
| **Prédiction heure de pointe** | Heure de pointe probable du lendemain (J-1 à J-7) | Moyenne glissante sur `PeakHour` des 4 dernières semaines, même jour |

---

## 4. Fonctionnalités analytics par plan

| Fonctionnalité | Basic (19€) | Pro (49€) | Enterprise (99€) |
|---|:---:|:---:|:---:|
| KPIs temps réel (file en cours) | ✅ | ✅ | ✅ |
| Résumé quotidien | ✅ | ✅ | ✅ |
| Historique 7 jours | ✅ | ✅ | ✅ |
| Historique 30 jours | ❌ | ✅ | ✅ |
| Historique 12 mois | ❌ | ❌ | ✅ |
| Export CSV | ❌ | ✅ | ✅ |
| Comparaison multi-établissements | ❌ | ✅ | ✅ |
| Taux de fidélité client | ❌ | ✅ | ✅ |
| Manque à gagner estimé | ❌ | ✅ | ✅ |
| Temps de réponse commerçant | ❌ | ✅ | ✅ |
| Prédiction heure de pointe | ❌ | ❌ | ✅ |
| Rapport PDF mensuel auto | ❌ | ❌ | ✅ |

---

## 5. Architecture technique

### 5.1 Vue d'ensemble

```
QueueEntries / SmsLogs
        │
        │  (événements temps réel)
        ▼
  [API ASP.NET]
        │
        ├── GET /analytics/live     → Calculs à la volée (PostgreSQL)
        │
        └── Job quotidien (minuit)
              │
              ▼
        AnalyticsDaily (INSERT/UPSERT)
              │
              ▼
        GET /analytics/daily|weekly|monthly  → Lecture directe
```

### 5.2 Job de calcul quotidien

Un job `CRON` s'exécute chaque nuit à **00:05** (heure de Paris) pour consolider les métriques de la veille dans `AnalyticsDaily`.

```csharp
// AnalyticsDailyJob.cs — exécuté via IHostedService ou Hangfire
public async Task ComputeDailyAnalyticsAsync(Guid businessId, DateOnly date)
{
    var entries = await _db.QueueEntries
        .Where(e => e.BusinessId == businessId
                 && e.CreatedAt.Date == date.ToDateTime(TimeOnly.MinValue).Date)
        .ToListAsync();

    var served    = entries.Count(e => e.Status == "served");
    var missed    = entries.Count(e => e.Status == "missed");
    var cancelled = entries.Count(e => e.Status == "cancelled");
    var total     = entries.Count;

    var avgWait    = entries.Where(e => e.Status == "served")
                            .Average(e => (double?)e.EstimatedWaitTime) ?? 0;
    var avgService = entries.Where(e => e.ActualServiceTime.HasValue)
                            .Average(e => (double?)e.ActualServiceTime) ?? 0;

    var peakHour = entries
        .GroupBy(e => e.CreatedAt.Hour)
        .OrderByDescending(g => g.Count())
        .Select(g => g.Key)
        .FirstOrDefault();

    var peakSize = entries
        .GroupBy(e => e.CreatedAt.Hour)
        .Max(g => g.Count());

    var abandonmentRate = total > 0
        ? (double)(missed + cancelled) / total * 100
        : 0;

    var smsCount = await _db.SmsLogs
        .CountAsync(s => s.BusinessId == businessId
                      && s.SentAt.HasValue
                      && s.SentAt.Value.Date == date.ToDateTime(TimeOnly.MinValue).Date);

    await _db.AnalyticsDaily.UpsertAsync(new AnalyticsDaily
    {
        BusinessId            = businessId,
        Date                  = date,
        TotalClientsRegistered = total,
        TotalClientsServed    = served,
        TotalClientsMissed    = missed,
        TotalClientsCancelled = cancelled,
        AverageWaitTime       = (decimal)avgWait,
        AverageServiceTime    = (decimal)avgService,
        PeakHour              = peakHour,
        PeakQueueSize         = peakSize,
        AbandonmentRate       = (decimal)abandonmentRate,
        SmsSentCount          = smsCount
    });
}
```

**Déclenchement** : Le job itère sur tous les `BusinessId` actifs. Il est idempotent (UPSERT sur `(BusinessId, Date)`).

### 5.3 Métriques temps réel

Les KPIs du jour sont calculés **à la volée** directement depuis `QueueEntries`, sans passer par `AnalyticsDaily`. Cela garantit une fraîcheur maximale.

```sql
-- Exemple : KPIs live pour un établissement
SELECT
    COUNT(*) FILTER (WHERE Status = 'waiting')                          AS clients_en_attente,
    COUNT(*) FILTER (WHERE Status = 'served' AND ServedAt::date = NOW()::date) AS servis_aujourd_hui,
    ROUND(AVG(EstimatedWaitTime) FILTER (WHERE Status = 'waiting'), 1)  AS attente_moyenne_actuelle
FROM QueueEntries
WHERE BusinessId = $1;
```

### 5.4 Export CSV (Pro / Enterprise)

L'export génère un fichier CSV côté serveur, streamé en réponse HTTP. Contenu : une ligne par jour sur la période demandée.

```csharp
// AnalyticsController.cs
[HttpGet("{businessId}/export")]
[Authorize(Policy = "ProOrEnterprise")]
public async Task<IActionResult> ExportCsv(Guid businessId, [FromQuery] DateOnly from, [FromQuery] DateOnly to)
{
    var rows = await _analyticsService.GetDailyRangeAsync(businessId, from, to);
    var csv  = CsvSerializer.Serialize(rows);
    return File(Encoding.UTF8.GetBytes(csv), "text/csv", $"waitify-{businessId}-{from}-{to}.csv");
}
```

---

## 6. Endpoints API

### 6.1 KPIs temps réel

```
GET /analytics/:businessId/live
```

**Auth** : Bearer token (commerçant propriétaire)

**Réponse :**

```json
{
  "businessId": "uuid",
  "updatedAt": "2026-04-05T14:32:00Z",
  "clientsWaiting": 4,
  "clientsServedToday": 23,
  "averageWaitMinutes": 8.5,
  "queueOpenSince": "2026-04-05T08:15:00Z"
}
```

---

### 6.2 Résumé quotidien

```
GET /analytics/:businessId/daily?date=2026-04-04
```

**Auth** : Bearer token

**Réponse :**

```json
{
  "date": "2026-04-04",
  "totalRegistered": 41,
  "totalServed": 35,
  "totalMissed": 3,
  "totalCancelled": 3,
  "serviceRate": 85.4,
  "abandonmentRate": 14.6,
  "averageWaitMinutes": 9.2,
  "averageServiceSeconds": 187,
  "peakHour": 11,
  "peakQueueSize": 12,
  "smsSentCount": 98
}
```

---

### 6.3 Tendance hebdomadaire

```
GET /analytics/:businessId/weekly?from=2026-03-30
```

**Auth** : Bearer token

**Réponse :**

```json
{
  "period": { "from": "2026-03-30", "to": "2026-04-05" },
  "days": [
    { "date": "2026-03-30", "totalServed": 28, "abandonmentRate": 12.0, "peakHour": 10 },
    { "date": "2026-03-31", "totalServed": 35, "abandonmentRate": 9.5,  "peakHour": 11 }
  ],
  "summary": {
    "totalServed": 210,
    "avgAbandonmentRate": 11.2,
    "bestDay": "thursday",
    "mostFrequentPeakHour": 11
  }
}
```

---

### 6.4 Tendance mensuelle

```
GET /analytics/:businessId/monthly?month=2026-03
```

**Auth** : Bearer token — **Plan Pro ou Enterprise requis**

Même structure que `/weekly` mais sur 30 jours, avec agrégation hebdomadaire dans `summary`.

---

### 6.5 Comparaison multi-établissements

```
GET /analytics/compare?businessIds=uuid1,uuid2,uuid3&period=weekly
```

**Auth** : Bearer token — **Plan Pro ou Enterprise requis**

**Réponse :**

```json
{
  "period": "weekly",
  "businesses": [
    { "businessId": "uuid1", "name": "Boulangerie Centre", "totalServed": 210, "abandonmentRate": 11.2 },
    { "businessId": "uuid2", "name": "Boulangerie Nord",   "totalServed": 174, "abandonmentRate": 18.7 }
  ]
}
```

---

### 6.6 Export CSV

```
GET /analytics/:businessId/export?from=2026-03-01&to=2026-03-31
```

**Auth** : Bearer token — **Plan Pro ou Enterprise requis**

**Réponse** : `Content-Type: text/csv`, fichier en téléchargement direct.

---

## 7. Schéma de base de données

### 7.1 `AnalyticsDaily` — déjà présente, aucune modification nécessaire

La table `AnalyticsDaily` couvre l'ensemble des métriques identifiées. Rappel des colonnes clés :

```sql
BusinessId             UUID        NOT NULL REFERENCES Businesses(Id)
Date                   DATE        NOT NULL
TotalClientsRegistered INTEGER     DEFAULT 0
TotalClientsServed     INTEGER     DEFAULT 0
TotalClientsMissed     INTEGER     DEFAULT 0
TotalClientsCancelled  INTEGER     DEFAULT 0
AverageWaitTime        DECIMAL     -- minutes
AverageServiceTime     DECIMAL     -- secondes
PeakHour               SMALLINT    -- 0-23
PeakQueueSize          INTEGER
AbandonmentRate        DECIMAL     -- pourcentage
SmsSentCount           INTEGER     DEFAULT 0
RevenuePotentialLost   INTEGER     -- centimes (Pro/Enterprise)
BusiestTimeStart       TIME
BusiestTimeEnd         TIME

UNIQUE (BusinessId, Date)
```

### 7.2 Index recommandés

```sql
-- Requêtes de tendance (range scan sur Date)
CREATE INDEX idx_analytics_daily_business_date
    ON AnalyticsDaily (BusinessId, Date DESC);

-- Agrégations multi-établissements
CREATE INDEX idx_analytics_daily_date
    ON AnalyticsDaily (Date DESC);
```

### 7.3 Vue SQL utile — taux de fidélité (Pro/Enterprise)

```sql
-- Clients revenant plusieurs fois chez un même établissement
CREATE VIEW v_returning_clients AS
SELECT
    BusinessId,
    Phone,
    COUNT(*)                            AS total_visits,
    MIN(CreatedAt)::date                AS first_visit,
    MAX(CreatedAt)::date                AS last_visit
FROM QueueEntries
WHERE Status IN ('served', 'called')
GROUP BY BusinessId, Phone
HAVING COUNT(*) > 1;
```

---

## 8. Calculs & formules

### Taux de service

```
TauxService = TotalClientsServed / TotalClientsRegistered × 100
```

Interprétation : >85 % = bonne gestion · 70–85 % = passable · <70 % = problème opérationnel.

### Taux d'abandon

```
TauxAbandon = (TotalClientsMissed + TotalClientsCancelled) / TotalClientsRegistered × 100
```

Déjà stocké dans `AbandonmentRate`. Un taux élevé peut signaler un `AverageServiceTime` sous-estimé ou une file trop longue.

### Temps de service réel moyen

```sql
SELECT AVG(ActualServiceTime)
FROM QueueEntries
WHERE BusinessId = $1
  AND Status = 'served'
  AND ActualServiceTime IS NOT NULL
  AND CreatedAt::date = $2;
```

### Manque à gagner estimé (`RevenuePotentialLost`)

```
MàG = (TotalClientsMissed + TotalClientsCancelled)
       × (AverageServiceTime / 3600)
       × tarifHoraireEstimé
```

`tarifHoraireEstimé` est une constante configurée par type d'établissement dans `SystemConfigs` (exemple : coiffeur = 40 €/h, boulangerie = 25 €/h). Valeur stockée en centimes dans `RevenuePotentialLost`.

### Prédiction heure de pointe (Enterprise)

```sql
-- Heure de pointe prédite pour demain (même jour de semaine, 4 dernières semaines)
SELECT MODE() WITHIN GROUP (ORDER BY PeakHour)
FROM AnalyticsDaily
WHERE BusinessId = $1
  AND EXTRACT(DOW FROM Date) = EXTRACT(DOW FROM NOW() + INTERVAL '1 day')
  AND Date >= NOW() - INTERVAL '28 days';
```

---

## 9. Règles UX & Design System

### Couleurs sémantiques pour les métriques

| Métrique | Couleur | Hex | Condition |
|---|---|---|---|
| Taux de service élevé | Succès | `#16A34A` | > 85 % |
| Taux de service moyen | Alerte | `#D97706` | 70–85 % |
| Taux de service faible | Erreur | `#DC2626` | < 70 % |
| Heure de pointe active | Accent | `#0EA5E9` | PeakHour = heure actuelle |
| Manque à gagner | Alerte | `#D97706` | Toujours |

### Règles d'affichage

- **Un chiffre clé par carte** — jamais deux métriques de même importance côte à côte
- **Horodatage toujours visible** — "Mis à jour il y a 2 min" en `TextSecondary` (`#6B7280`) sous chaque KPI temps réel
- **Pas de graphes sur mobile** — remplacer par des badges et des variations `+X% vs hier`
- **Verbe d'action dans les insights** — "Ta file a été 18 % plus efficace qu'hier" plutôt que "AbandonmentRate : −18%"
- **Accès plan supérieur** — les métriques Pro/Enterprise non accessibles affichent un état `disabled` avec le label du plan requis, jamais une erreur 403 silencieuse

### Période de rafraîchissement

| Vue | Fréquence | Source |
|---|---|---|
| KPIs temps réel | Polling 60s | `QueueEntries` (requête directe) |
| Résumé du jour (en cours) | Polling 5 min | `QueueEntries` (requête directe) |
| Résumé du jour (passé) | Statique | `AnalyticsDaily` |
| Hebdomadaire / Mensuel | Statique | `AnalyticsDaily` |

---

_Document maintenu en parallèle de `DOCUMENTATION_TECHNIQUE.md` et du `Design System v1.0.0`.
Toute évolution du schéma `QueueEntries` ou `AnalyticsDaily` doit se refléter ici._
