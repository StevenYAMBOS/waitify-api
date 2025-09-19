# Documentation

Mise à jour le : 19-09-2025

Par : Steven YAMBOS

## ⚠️ Organisation du dépôt

L'organisation des branches du dépôt est structurée pour faciliter le développement, les tests, et le déploiement en production. Voici les principales branches utilisées :

- **`main`** :
  Point d'entrée du projet, contient tous les documents nécessaires à la compréhension du projet.

- **`dev`** (vous vous trouvez ici) :
  La branche principale de développement continu. Elle sert d'environnement bac à sable pour les développeurs où toutes les nouvelles fonctionnalités et corrections de bugs sont intégrées après validation initiale.

- **`pre-prod`** :
  Cette branche est destinée à présenter les fonctionnalités aux clients. Une fois que les développements de la branche `dev` sont stabilisés et validés, ils sont fusionnés dans cette branche pour des démonstrations.

- **`prod`** :
  La branche finale de production qui contient la version stable et prête à être déployée de l'application. Elle est mise à jour uniquement lorsque les changements dans `pre-prod` sont entièrement validés.

**Bonnes pratiques :**

- Tester les fonctionnalités dans la branche `dev` avant de les intégrer dans `pre-prod`.
- Ne jamais effectuer de développement direct sur les branches `pre-prod` et `prod`.
- Maintenir la branche `prod` uniquement avec du code stable et prêt pour les utilisateurs finaux.
