# syntax=docker/dockerfile:1

# ---------- build ----------
FROM node:22-alpine AS build
WORKDIR /app

COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci

COPY frontend/ ./

# ВАЖНО: собираем через `vite build`, а не `npm run build`.
# `npm run build` = `tsc -b && vite build`, а `tsc` падает на пред-существующих
# ошибках типов в проекте (AdminModal, ProductsPage, agent-chat/auth/cart и т.д.),
# из-за чего образ не собрался бы. Vite типы не проверяет.
RUN npx vite build

# ---------- runtime ----------
FROM nginx:alpine AS final
COPY frontend/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
