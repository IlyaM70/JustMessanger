# --- Stage 1: build with Node ---
FROM node:18-alpine AS build
WORKDIR /app

# install dependencies (use package*.json for lockfile support)
COPY package*.json ./
RUN npm ci

# copy source and build
COPY . .
RUN npm run build

# --- Stage 2: serve with nginx ---
FROM nginx:alpine AS runtime
# Remove default config and write an SPA-friendly one
RUN rm /etc/nginx/conf.d/default.conf
RUN cat > /etc/nginx/conf.d/default.conf <<'EOF'
server {
    listen 80;
    server_name  localhost;
    root /usr/share/nginx/html;

    index index.html;

    # Serve static files directly, fallback to index.html (SPA)
    location / {
        try_files $uri $uri/ /index.html;
    }

    # Optional: reduce logging noise (comment out if you want logs)
    access_log  /var/log/nginx/access.log  main;
    error_log   /var/log/nginx/error.log  warn;
}
EOF

# copy built assets
COPY --from=build /app/dist /usr/share/nginx/html

# expose port (container)
EXPOSE 80

# run nginx in foreground
CMD ["nginx", "-g", "daemon off;"]
