// src/common/guards/privy-auth.guard.ts
import { Injectable, CanActivate, ExecutionContext, UnauthorizedException } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';

@Injectable()
export class PrivyAuthGuard implements CanActivate {
  constructor(private configService: ConfigService) {}

  private getVerifyFunction(): any {
    // Try to require the module and check for common export names
    const privyModule = require('@privy-io/server-auth');
    
    // Check for different possible function names
    if (privyModule.verifyToken) {
      return privyModule.verifyToken;
    } else if (privyModule.verify) {
      return privyModule.verify;
    } else if (privyModule.verifyPrivyToken) {
      return privyModule.verifyPrivyToken;
    } else if (privyModule.default) {
      // If the module uses a default export
      return privyModule.default;
    } else {
      throw new Error('Could not find a verify function in @privy-io/server-auth');
    }
  }

  async canActivate(context: ExecutionContext): Promise<boolean> {
    const request = context.switchToHttp().getRequest();
    const authHeader = request.headers.authorization;
    
    if (!authHeader) {
      throw new UnauthorizedException('No authorization token provided');
    }
    
    try {
      const token = authHeader.replace('Bearer ', '');
      const appSecret = this.configService.get('privy.appSecret');
      
      if (!appSecret) {
        throw new Error('Privy app secret not configured');
      }
      
      const verifyFunction = this.getVerifyFunction();
      const verifiedClaims = await verifyFunction(appSecret, token);
      request.user = verifiedClaims;
      return true;
    } catch (error) {
      console.error('Privy authentication error:', error);
      throw new UnauthorizedException('Invalid or expired token');
    }
  }
}