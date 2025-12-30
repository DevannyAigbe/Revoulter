// src/modules/users/users.service.ts
import { Injectable, Logger, NotFoundException } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { User } from './entities/user.entity';

@Injectable()
export class UsersService {
  private readonly logger = new Logger(UsersService.name);

  constructor(
    @InjectRepository(User)
    private usersRepository: Repository<User>,
  ) {}

  // This will be called automatically when a user first authenticates
  async findOrCreate(privyClaims: any): Promise<User> {
    const privyId = privyClaims.sub || privyClaims.userId;
    
    let user = await this.usersRepository.findOne({
      where: { privyId },
    });

    if (!user) {
      this.logger.log(`Creating new user: ${privyId}`);
      user = this.usersRepository.create({
        privyId,
        walletAddress: privyClaims.wallet?.address,
        email: privyClaims.email?.address,
        userMetadata: privyClaims,
      });
      await this.usersRepository.save(user);
    } else {
      this.logger.log(`User found: ${privyId}`);
    }

    return user;
  }

  async findById(privyId: string): Promise<User | null> {
    return this.usersRepository.findOne({
      where: { privyId },
      relations: ['ipAssets'],
    });
  }

  // Add this missing save method
  async save(user: User): Promise<User> {
    return this.usersRepository.save(user);
  }
}