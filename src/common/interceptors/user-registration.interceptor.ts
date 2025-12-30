// src/common/interceptors/user-registration.interceptor.ts
import {
  Injectable,
  NestInterceptor,
  ExecutionContext,
  CallHandler,
} from '@nestjs/common';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { UsersService } from '../../users/users.service';

@Injectable()
export class UserRegistrationInterceptor implements NestInterceptor {
  constructor(private readonly usersService: UsersService) {}

  async intercept(context: ExecutionContext, next: CallHandler): Promise<Observable<any>> {
    const request = context.switchToHttp().getRequest();
    
    // Auto-register user if authenticated
    if (request.user) {
      await this.usersService.findOrCreate(request.user);
    }
    
    return next.handle();
  }
}