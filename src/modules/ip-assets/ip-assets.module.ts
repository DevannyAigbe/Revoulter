// src/modules/ip-assets/ip-assets.module.ts
import { Module } from '@nestjs/common';
import { TypeOrmModule } from '@nestjs/typeorm';
import { IpAssetsService } from './ip-assets.service';
import { IpAssetsController } from './ip-assets.controller';
import { IPAsset } from './entities/ip-asset.entity';
import { ArweaveModule } from '../arweave/arweave.module';
import { StoryProtocolModule } from '../story-protocol/story-protocol.module';
import { UsersModule } from '../users/users.module';

@Module({
  imports: [
    TypeOrmModule.forFeature([IPAsset]),
    ArweaveModule,        // This provides ArweaveService
    StoryProtocolModule,  // This provides StoryProtocolService
    UsersModule,          // This provides UsersService
  ],
  controllers: [IpAssetsController],
  providers: [IpAssetsService],
  exports: [IpAssetsService],
})
export class IpAssetsModule {}