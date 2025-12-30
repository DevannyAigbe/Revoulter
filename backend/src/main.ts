import { NestFactory } from '@nestjs/core';
import { AppModule } from './app.module'
import * as dotenv from 'dotenv';

dotenv.config();

async function bootstrap() {
  const app = await NestFactory.create(AppModule);

  // Enable CORS
  app.enableCors({
    origin: true, // Allows any origin (good for development + Docker)
    credentials: true, // If you ever use cookies/auth
  });

  
  await app.listen(process.env.PORT ?? 3000);
}
bootstrap();
