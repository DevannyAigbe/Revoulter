// src/auth.middleware.ts
import { Injectable, NestMiddleware, UnauthorizedException } from '@nestjs/common';
import type { Request, Response, NextFunction } from 'express';

const privy = require('@privy-io/server-auth');
const verifyToken = privy.verifyPrivyToken || privy.verify;

@Injectable()
export class AuthMiddleware implements NestMiddleware {
  async use(req: Request, res: Response, next: NextFunction) {
    const authHeader = req.headers.authorization;
    if (!authHeader) {
      throw new UnauthorizedException('No authorization token provided');
    }

    try {
      const token = authHeader.replace('Bearer ', '');
      const appSecret = process.env.PRIVY_APP_SECRET;
      
      if (!appSecret) {
        throw new Error('Privy app secret not configured');
      }

      if (!verifyToken) {
        throw new Error('Privy token verification function not available');
      }

      const verifiedClaims = await verifyToken(appSecret, token);
      
      // Type assertion to our interface
      req.user = verifiedClaims as any;
      next();
    } catch (error) {
      console.error('Privy authentication error:', error);
      throw new UnauthorizedException('Invalid or expired token');
    }
  }
}