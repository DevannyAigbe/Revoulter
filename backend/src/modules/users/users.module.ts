// src/modules/users/users.module.ts
import { Module, Global } from '@nestjs/common';
import { TypeOrmModule } from '@nestjs/typeorm';
import { APP_INTERCEPTOR } from '@nestjs/core';
import { UsersService } from '../../users/users.service';
import { UsersController } from '../../users/users.controller';
import { User } from '../../users/entities/user.entity';
import { UserRegistrationInterceptor } from '../../common/interceptors/user-registration.interceptor';

@Global() // Make this module global so interceptor works everywhere
@Module({
  imports: [TypeOrmModule.forFeature([User])],
  controllers: [UsersController],
  providers: [
    UsersService,
    {
      provide: APP_INTERCEPTOR,
      useClass: UserRegistrationInterceptor,
    },
  ],
  exports: [UsersService],
})
export class UsersModule {}