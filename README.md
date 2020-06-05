
# ASP.NET Core Forum

Le projet **ASP.NET Core Forum** consiste à fournir un forum **complet** et **fonctionnel**. Il content une API RestFul développée en ASP.NET Core, et un front-end développé en Angular. 

# Acteurs (rôles)

 - Utilisateur non connecté
 - Utilisateur connecté
 - Modérateur
 - Administrateur

# Fonctionnalités
Le projet se découpe en plusieurs fonctionnalités disponibles pour plusieurs rôles différents :

**Utilisateur non inscrit:**

 - S'inscrire
 - Valider son compte
 - Visualiser les catégories publiques
 - Contacter les administrateurs

**Utilisateur inscrit non connecté**

 - Se connecter
 - Récupérer son mot de passe

**Utilisateur connecté:**

 - Se déconnecter
 - Accéder à son profil
 - Modifier son profil
 - Supprimer son profil
 - Afficher le profil d'autres utilisateurs
 - Visualiser toutes les catégories
 - Créer des posts dans les catégories
 - Visualiser les posts et les réponses
 - Répondre aux posts du forum
 - Insérer des images à ses réponses
 - Participer à la discussion sur le chat
 - Supprimer ses posts
 - Supprimer ses réponses aux posts
 - Déplacer ses posts
 - Visualiser ses messages privés
 - Envoyer des messages privés à d'autres utilisateurs
 - Supprimer ses conversations privées
 - Recevoir des notifications lors d'une réponse à un post ou un message privé
 - Verrouiller ses posts
 - Déverrouiller ses posts

**Modérateur:**

 - Supprimer des posts créés par des utilisateurs simples
 - Déplacer des posts d'autres utilisateurs dans d'autres catégories
 - Supprimer des messages dans le chat
 - Bannir des utilisateurs
 - Bannir des utilisateurs depuis le chat
 - Verrouiller des posts
 - Déverrouiller des posts
 
 **Administrateur:**
 
 - Accéder à la gestion des utilisateurs
 - Filtrer la liste des utilisateurs (tous, bannis, non bannis)
 - Modifier des utilisateurs
 - Changer le rôle des utilisateurs (Utilisateur Simple, Modérateur, Administrateur)
 - Afficher la liste des catégories
 - Créer des catégories
 - Modifier des catégories
 - Supprimer des catégories
 - Débannir des utilisateurs

## Développé avec

* [ASP.NET Core 3.1](https://docs.microsoft.com/fr-fr/aspnet/core/?view=aspnetcore-3.1) - API RestFul
* [Angular JS](https://angular.io/) - Framework Front-End