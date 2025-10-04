## Logique du système de file d'attente

### QR Code : Fonctionnement et unicité

**Un QR Code = Un établissement (business)**

Le QR Code n'est pas lié à une file d'attente spécifique mais à l'établissement lui-même. La file d'attente est temporaire et dynamique, le QR Code est permanent.

**Pourquoi ?**

- Le commerçant affiche un seul QR Code physique
- Les clients scannent toujours le même QR Code
- La file d'attente se "construit" au fur et à mesure des scans

### Génération du QR Code

**Quand : À la création de l'entreprise**

```
POST /businesses
→ Création du business dans PostgreSQL
→ Génération automatique du qr_code_token (colonne existante)
→ Retour de l'URL du QR Code au commerçant
```

**Comment générer le token :**

1. Utiliser `uuid_generate_v4()` de PostgreSQL (déjà dans le schéma)
2. Stocker dans la colonne `qr_code_token` de la table `businesses`
3. Construire l'URL : `https://app.waitify.fr/q/{qr_code_token}`

**Le QR Code encode simplement cette URL.**

Le commerçant reçoit un PDF/PNG du QR Code à imprimer et afficher.

### Cycle de vie de la file d'attente

**La file d'attente n'est PAS créée explicitement**

Elle existe implicitement via les entrées dans `queue_entries`.

**Flux complet :**

1. **Commerçant** : Crée son business → QR Code généré automatiquement
2. **Commerçant** : Active la file via `PUT /businesses/:id/queue/activate`
3. **Client** : Scanne le QR Code → Redirigé vers `https://app.waitify.fr/q/{token}`
4. **Client** : Page web affiche le formulaire (nom + téléphone)
5. **Client** : Soumet le formulaire → `POST /queue/join`
6. **API** :
   - Vérifie que `is_queue_active = true` pour ce business
   - Calcule la prochaine position disponible
   - Insère une nouvelle ligne dans `queue_entries`
   - Envoie SMS de confirmation
7. **Client** : Reçoit sa position et le temps d'attente estimé

**Il n'y a pas d'objet "file d'attente" en base.**

La file d'attente = l'ensemble des `queue_entries` avec `status = 'waiting'` pour un `BusinessId` donné.

### Architecture des endpoints

```markdown
# Côté commerçant (authentifié)
PUT  /businesses/:id/queue/activate   # Ouvrir la file
PUT  /businesses/:id/queue/deactivate # Fermer la file
POST /businesses/:id/queue/next       # Appeler le client suivant

# Côté client (public, via QR Code)
GET  /queue/info/:token               # Infos du business (nom, temps moyen)
POST /queue/join                      # S'inscrire dans la file
GET  /queue/status/:entryId           # Voir sa position
DELETE /queue/cancel/:entryId         # Annuler sa place
```

### Points d'attention

**Validation à l'inscription :**

- Vérifier que `is_queue_active = true`
- Vérifier que la file n'est pas pleine (`max_queue_size`)
- Vérifier que le client n'est pas déjà inscrit (même phone + BusinessId + status='waiting')

**Calcul de la position :**

```sql
SELECT COALESCE(MAX(position), 0) + 1 
FROM queue_entries 
WHERE BusinessId = ? AND status = 'waiting';
```

**Temps d'attente estimé :**

```
position * average_service_time / 60
```

Votre architecture actuelle supporte déjà tout cela. Il vous faut créer :

- `internal/handlers/queueHandlers.go`
- `internal/models/queueModels.go`
- Les routes correspondantes dans `cmd/main.go`
