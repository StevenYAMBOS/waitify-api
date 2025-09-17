FROM golang:1.24.3

# Travailler dans le dossier `/app`
# Dans le conteneur le dossier `/app` va contenir le code du projet
WORKDIR /app

# Copier les fichiers des dépendances (go.mod, go.sum) dans le dossier `/app`
COPY go.mod go.sum ./

# Installer les dépendances
RUN go mod download

# Copier le code entier dans le conteneur
COPY . /app

# Build l'app
RUN cd cmd go build -o waitify_exec

# Port
EXPOSE 3000

# Lancer l'application
CMD ["/app/waitify_exec"]
