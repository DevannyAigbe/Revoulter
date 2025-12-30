// src/app.module.ts
import { Module, NestModule, MiddlewareConsumer } from '@nestjs/common';
import { TypeOrmModule } from '@nestjs/typeorm';
import { ConfigModule, ConfigService } from '@nestjs/config';
import { ThrottlerModule } from '@nestjs/throttler';

import { AppController } from './app.controller';
import { AppService } from './app.service';
import { AuthMiddleware } from './auth.middleware';
import { UsersModule } from './modules/users/users.module';
import { IpAssetsModule } from './modules/ip-assets/ip-assets.module';
import { ArweaveModule } from './modules/arweave/arweave.module';
import { StoryProtocolModule } from './modules/story-protocol/story-protocol.module';

@Module({
  imports: [
    // Configuration - FIXED: Use undefined instead of null
    ConfigModule.forRoot({
      isGlobal: true,
      envFilePath: process.env.NODE_ENV === 'development' ? '.env' : undefined,
    }),
    
    // Database - Fix SSL for production
    TypeOrmModule.forRootAsync({
      imports: [ConfigModule],
      useFactory: (configService: ConfigService) => {
        const isProduction = configService.get<string>('NODE_ENV') === 'production';
        
        return {
          type: 'postgres',
          host: configService.get<string>('DATABASE_HOST') || 'localhost',
          port: parseInt(configService.get<string>('DATABASE_PORT') || '5432', 10),
          username: configService.get<string>('DATABASE_USER') || 'postgres',
          password: configService.get<string>('DATABASE_PASSWORD') || 'password',
          database: configService.get<string>('DATABASE_NAME') || 'ip_platform',
          entities: [__dirname + '/**/*.entity{.ts,.js}'],
          synchronize: configService.get<string>('NODE_ENV') !== 'production',
          logging: configService.get<string>('NODE_ENV') !== 'production',
          // ADD THIS FOR RENDER POSTGRES
          ssl: isProduction ? {
            rejectUnauthorized: false
          } : false,
        };
      },
      inject: [ConfigService],
    }),
    
    // Rate limiting
    ThrottlerModule.forRoot([{
      ttl: 60000,
      limit: 100,
    }]),
    
    // Your new modules
    UsersModule,
    IpAssetsModule,
    ArweaveModule,
    StoryProtocolModule,
  ],
  controllers: [AppController],
  providers: [AppService],
})
export class AppModule implements NestModule {
  configure(consumer: MiddlewareConsumer) {
    // Apply AuthMiddleware to ALL routes (or specific ones as needed)
    consumer
      .apply(AuthMiddleware)
      .exclude(
        'health', // Public health check endpoint
      )
      .forRoutes('*');
  }
}