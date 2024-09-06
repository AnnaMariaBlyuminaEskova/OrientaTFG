# Stage 0, based on Node.js, to build and compile Angular
FROM node:latest as build
WORKDIR /app
COPY ./ /app/
RUN npm install

RUN npm run build 

# Stage 1, based on Nginx, to have only the compiled app, ready for production with Nginx
FROM nginx:latest
COPY --from=build /app/dist/orientatfg.frontend.client/browser /usr/share/nginx/html
COPY ./nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]