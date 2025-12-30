// src/modules/users/users.controller.ts (with auto-creation)
import { Controller, Get, Req, Put, Body } from '@nestjs/common';
import { UsersService } from './users.service';

@Controller('users')
export class UsersController {
  constructor(private readonly usersService: UsersService) {}

  @Get('me')
  async getMe(@Req() req: any) {
    const user = await this.usersService.findOrCreate(req.user);
    
    return {
      ...user,
      // Don't expose sensitive data
      userMetadata: undefined,
    };
  }

  @Put('preferences')
  async updatePreferences(@Req() req: any, @Body() preferences: any) {
    const user = await this.usersService.findOrCreate(req.user);
    
    if (preferences.externalServicesEnabled !== undefined) {
      user.externalServicesEnabled = preferences.externalServicesEnabled;
      await this.usersService.save(user);
    }
    
    return { success: true };
  }
}