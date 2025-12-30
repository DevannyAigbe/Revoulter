// src/app.controller.ts (Enhanced with health check)
import { Controller, Get, Req } from '@nestjs/common';
import { AppService } from './app.service';
import type { Request } from 'express';

@Controller()
export class AppController {
  constructor(private readonly appService: AppService) {}

  @Get()
  getHello(): string {
    return this.appService.getHello();
  }

  @Get('health')
  getHealth(): { status: string; timestamp: string } {
    return {
      status: 'ok',
      timestamp: new Date().toISOString(),
    };
  }

  @Get('user')
  getUser(@Req() req: Request): { userId: string; wallet?: string } {
    const claims = req.user as any;
    return { 
      userId: claims.sub || claims.userId,
      wallet: claims.wallet?.address 
    };
  }
}