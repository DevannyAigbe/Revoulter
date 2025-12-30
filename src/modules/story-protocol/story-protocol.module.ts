// src/modules/story-protocol/story-protocol.module.ts
import { Module } from '@nestjs/common';
import { ConfigModule } from '@nestjs/config';
import { StoryProtocolService } from './story-protocol.service';

@Module({
  imports: [ConfigModule],
  providers: [StoryProtocolService],
  exports: [StoryProtocolService],
})
export class StoryProtocolModule {}