FROM node:20-alpine AS build
WORKDIR /app

COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci

COPY frontend/ ./

ARG VITE_AUTH_ENABLED=false
ARG VITE_OIDC_AUTHORITY=""
ARG VITE_OIDC_CLIENT_ID=""
ARG VITE_OIDC_SCOPE="openid profile email"
ARG VITE_OIDC_AUDIENCE=""
ENV VITE_AUTH_ENABLED=$VITE_AUTH_ENABLED \
    VITE_OIDC_AUTHORITY=$VITE_OIDC_AUTHORITY \
    VITE_OIDC_CLIENT_ID=$VITE_OIDC_CLIENT_ID \
    VITE_OIDC_SCOPE=$VITE_OIDC_SCOPE \
    VITE_OIDC_AUDIENCE=$VITE_OIDC_AUDIENCE

RUN npm run build

FROM nginx:1.27-alpine AS final
COPY frontend/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80

HEALTHCHECK --interval=10s --timeout=3s --start-period=5s --retries=5 \
    CMD wget -q -O- http://127.0.0.1/ >/dev/null || exit 1
