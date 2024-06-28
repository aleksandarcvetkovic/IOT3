# Use the official Node.js image as the base image
FROM node:latest

# Set the working directory inside the container
WORKDIR /usr/src/app

# Copy package.json and package-lock.json to the working directory
COPY package*.json ./

# Install dependencies
RUN npm install



# Copy the rest of the application code to the working directory
COPY . .

EXPOSE 3001

CMD ["node", "Server.js"]