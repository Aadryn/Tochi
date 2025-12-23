FROM mcr.microsoft.com/dotnet/aspnet:9.0

# Installer nginx et nettoyer le cache apt
RUN apt-get update -y && apt-get upgrade -y && apt-get install nginx -y \
    && apt-get clean && rm -rf /var/lib/apt/lists/*

# Créer le répertoire pour les logs nginx
RUN mkdir -p /var/log/nginx

# Copier la configuration nginx (optionnel, sinon utilise la config par défaut)
# COPY nginx.conf /etc/nginx/nginx.conf

# Exposer le port 80
EXPOSE 80

# Copier le script de démarrage
COPY startup.sh /startup.sh
RUN chmod +x /startup.sh

# Définir le point d'entrée
CMD ["/startup.sh"]
