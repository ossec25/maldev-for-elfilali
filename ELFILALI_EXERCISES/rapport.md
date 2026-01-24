Yassine EL FILALI 

24/01/2026

# Activité : Développement de Malware

## 1. Introduction
Ce laboratoire a pour objectif d'explorer les mécanismes utilisés par les logiciels malveillants pour s'exécuter et se maintenir sur un système Windows. Le but est de comprendre comment les attaquants manipulent la mémoire et les processus afin de mieux appréhender les mécanismes de défense et de détection.

## 2. Exercice 0 : Environnement de Travail

Pour mener à bien les différents exercices, l'architecture suivante a été mise en place :

* **Machine Attaquante :**  WSL
    * Outil utilisé : `msfvenom` pour la génération de la charge utile.
* **Machine de Développement :** Windows 11.
* **Machine de Testing :** Windows 11 (Antivirus désactivé).
* **IDE :** Visual Studio 2019.
* **Langage :** C# (.NET Framework 4.7.2).

**Génération du Shellcode :**
Le shellcode a été généré spécifiquement pour l'architecture x64, afin de afficher une messageBox. La commande utilisée est :

```bash
msfvenom -p windows/x64/messagebox TEXT='Task failed successfully!' TITLE='Error!' -f csharp
```

![alt text](0-1.png)

## 3. Exercice 1 : Loader

### 3.1. Objectif
L'objectif de cet exercice était de créer un programme capable d'allouer de la mémoire et d'y exécuter un shellcode.

### 3.2. Implémentation Technique
Le programme utilise **P/Invoke** pour importer les fonctions de l'API Windows (`kernel32.dll`). La séquence d'exécution est la suivante :

1.  **Allocation :** Appel à `VirtualAlloc` pour allouer une zone mémoire avec les permissions `EXECUTE_READWRITE`.
2.  **Copie :** Utilisation de `Marshal.Copy` pour transférer le shellcode vers cette zone mémoire allouée.
3.  **Exécution :** Appel à `CreateThread` pour lancer une nouvelle exécution pointant vers l'adresse du shellcode.
4.  **Attente :** Utilisation de `WaitForSingleObject` pour empêcher la fermeture du programme principal, ce qui tuerait le thread malveillant.

### 3.3. Difficultés Rencontrées et Résolution
* **Problème :** Lors des premiers essais tests, le programme plantait.
* **Analyse :** Visual Studio compile par défaut en mode "Any CPU", alors que le shellcode généré par msfvenom était du **x64**.
* **Résolution :** J'ai forcé la configuration de la solution en mode **x64** via le Gestionnaire de configuration de Visual Studio.

### 3.4. Exécution
> ![alt text](3-4.png)

## 4. Exercice 2 : Process Injection

### 4.1. Objectif
L'objectif était d'injecter le shellcode dans un processus tiers (ici `notepad.exe`). Cette technique permet de cacher l'activité malveillante sous l'identité d'un programme de confiance.

### 4.2. Implémentation Technique
Nous devons manipuler la mémoire d'un autre processus. La séquence d'exécution est la suivante :

1.  **Ciblage :** `Process.GetProcessesByName("notepad")` pour récupérer l'ID du processus cible.
2.  **Ouverture :** `OpenProcess` pour obtenir un **Handle** avec tous les droits.
3.  **Allocation Distante :** `VirtualAllocEx` pour allouer de la mémoire dans le processus cible.
4.  **Écriture :** `WriteProcessMemory` pour copier le shellcode depuis notre injecteur vers le Notepad.
5.  **Exécution :** `CreateRemoteThread` pour démarrer l'exécution du shellcode dans le Notepad.

### 4.3. Observations
Une fois l'injection réalisée, l'injecteur se termine. Le payload s'exécute dans `notepad.exe`. Dans le Gestionnaire des tâches, l'utilisation CPU apparaît sous le nom "Notepad", alors que nous avons un code malveillant qui tourne dedans.

### 4.4. Exécution
> ![alt text](4-4.png)

## 5. Exercice 3 : Évasion et Obfuscation

### 5.1. Objectif
Contourner la détection de l'antivirus. Les exercices précédents contenaient le shellcode en clair.

### 5.2. Implémentation Technique
J'ai choisi une approche combinant le chiffrement XOR et l'encodage Base64. 

La stratégie repose sur deux transformations :
1.  **XOR :** Applique un masque avec une clé secrète pour rendre le shellcode méconnaissable.
2.  **Base64 :** Transforme le résultat binaire en une chaîne de caractères ASCII standard

J'ai développé une classe utilitaire contenant deux méthodes pour le chiffrement et déchiffrement.

1. **Chiffrement :** Initialise une propriété avec le shellcode chiffré.
2.  **Ciblage :** `Process.GetProcessesByName("notepad")` pour récupérer l'ID du processus cible.
3.  **Ouverture :** `OpenProcess` pour obtenir un **Handle** avec tous les droits.
4. **Déchiffrement :** Déchiffre le shellcode encrypté pour pouvoir l'utiliser.
5.  **Allocation Distante :** `VirtualAllocEx` pour allouer de la mémoire dans le processus cible.
6.  **Écriture :** `WriteProcessMemory` pour copier le shellcode depuis notre injecteur vers le Notepad.
7.  **Exécution :** `CreateRemoteThread` pour démarrer l'exécution du shellcode dans le Notepad.

### 5.3. Observations
Une fois l'injection réalisée, l'injecteur se termine. Le payload s'exécute dans `notepad.exe`. Dans le Gestionnaire des tâches, l'utilisation CPU apparaît sous le nom "Notepad", alors que nous avons un code malveillant qui tourne dedans.

### 5.4. Exécution
> ![alt text](5-4.png)

## 6. Bonus 01 : Exécution du Loader sans CreateThread

### 6.1. Objectif
L'appel `CreateThread` est très surveillé par les antivirus. L'objectif est d'exécuter le shellcode dans le processus courant sans créer explicitement de thread d'exécution.

### 6.2. Implémentation Technique
Au lieu d'utiliser l'API Windows pour gérer l'exécution, j'ai utilisé une solution inclus dans le langage C# et son framework .NET : le **Marshaling Delegate**.

La fonction `Marshal.GetDelegateForFunctionPointer` permet de convertir l'adresse mémoire de notre shellcode alloué via `VirtualAlloc` en une fonction appelable.

Le code ne nécessite plus l'importation de `CreateThread`.

### 6.3. Exécution
> ![alt text](6-3.png)

## 7. Bonus 02 : Process Injection Autonome

### 7.1. Objectif
L'injecteur développé dans l'Exercice 2 nécessitait que le processus cible (ex: `notepad.exe`) soit déjà lancé par l'utilisateur. Si la cible était absente, le programme se terminait.
L'objectif de ce bonus est de rendre l'injecteur autonome : il doit vérifier la présence du processus cible et le démarrer lui-même s'il est absent.

### 7.2. Implémentation Technique
J'ai modifié le flux d'exécution pour ajouter une vérification conditionnelle avant l'ouverture du Handle :

1.  **Recherche :** Tentative de récupération du processus via `Process.GetProcessesByName`.
2.  **Condition :**
    * **Si trouvé :** On utilise le PID existant.
    * **Si non trouvé :** On instancie un nouvel objet `Process`, on configure le nom du fichier exécutable, et on le démarre via la méthode `.Start()`.
3.  **Ouverture :** `OpenProcess` pour obtenir un **Handle** avec tous les droits.
4.  **Allocation Distante :** `VirtualAllocEx` pour allouer de la mémoire dans le processus cible.
5.  **Écriture :** `WriteProcessMemory` pour copier le shellcode depuis notre injecteur vers le Notepad.
6.  **Exécution :** `CreateRemoteThread` pour démarrer l'exécution du shellcode dans le Notepad.

// Suite standard : OpenProcess -> VirtualAllocEx -> Write -> RemoteThread

### 7.3. Exécution
> ![alt text](7-30.png)

> ![alt text](7-31.png)

