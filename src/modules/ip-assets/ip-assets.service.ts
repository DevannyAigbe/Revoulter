// src/modules/ip-assets/ip-assets.service.ts
import { Injectable, BadRequestException, NotFoundException } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { ConfigService } from '@nestjs/config';
import { createHash } from 'crypto';
import * as fs from 'fs/promises';
import * as path from 'path';

import { IPAsset } from './entities/ip-asset.entity';
import { User } from '../../users/entities/user.entity';
import { ArweaveService } from '../arweave/arweave.service';
import { StoryProtocolService } from '../story-protocol/story-protocol.service';

@Injectable()
export class IpAssetsService {
  constructor(
    @InjectRepository(IPAsset)
    private ipAssetsRepository: Repository<IPAsset>,
    private configService: ConfigService,
    private arweaveService: ArweaveService,
    private storyProtocolService: StoryProtocolService,
  ) {}

  async create(
    createDto: any,
    file: Express.Multer.File,
    user: User,
  ): Promise<IPAsset> {
    // Validate file
    this.validateFile(file);

    // Save file to uploads directory
    const uploadDir = this.configService.get<string>('app.uploadDir') || './uploads';
    const fileHash = createHash('sha256').update(file.buffer).digest('hex');
    const fileName = `${fileHash}-${Date.now()}${path.extname(file.originalname)}`;
    const filePath = path.join(uploadDir, fileName);
    
    await fs.writeFile(filePath, file.buffer);

    let externalData: { arweaveId: string; ipAssetId: string; txHash: string } | null = null;
    let status = 'mock';

    // Check if we should use external services
    const shouldUseExternal = createDto.useExternalServices && 
                              user.externalServicesEnabled &&
                              this.configService.get('app.mode') !== 'mock-only';

    if (shouldUseExternal) {
      try {
        externalData = await this.publishToExternalServices(
          file, 
          createDto.title,
          user
        );
        status = 'published';
      } catch (error) {
        console.warn('External services failed, falling back to mock:', error);
        status = 'failed';
        // Fall back to mock - continue saving to database
      }
    } else {
      status = 'mock';
    }

    // Create IP Asset record - Fixed to match entity types
    const ipAssetData = {
      title: createDto.title,
      fileName: file.originalname,
      filePath: fileName, // Store relative path
      fileType: file.mimetype,
      fileSize: file.size,
      arweaveId: externalData?.arweaveId || undefined, // Use undefined instead of null
      ipAssetId: externalData?.ipAssetId || undefined, // Use undefined instead of null
      transactionHash: externalData?.txHash || undefined, // Use undefined instead of null
      status,
      publishedAt: externalData ? new Date() : undefined, // Use undefined instead of null
      user,
      userId: user.privyId,
      metadata: {
        originalName: file.originalname,
        size: file.size,
        mimeType: file.mimetype,
        hash: fileHash,
        uploadedAt: new Date().toISOString(),
      },
    };

    const ipAsset = this.ipAssetsRepository.create(ipAssetData);
    return this.ipAssetsRepository.save(ipAsset);
  }

  async findAllByUser(userId: string): Promise<IPAsset[]> {
    return this.ipAssetsRepository.find({
      where: { userId },
      order: { createdAt: 'DESC' },
    });
  }

  async findOne(id: string, userId: string): Promise<IPAsset> {
    const ipAsset = await this.ipAssetsRepository.findOne({
      where: { id, userId },
      relations: ['user'], // Make sure user relation is loaded
    });

    if (!ipAsset) {
      throw new NotFoundException(`IP Asset with ID ${id} not found`);
    }

    return ipAsset;
  }

  async syncWithExternalServices(id: string, userId: string): Promise<IPAsset> {
    const ipAsset = await this.findOne(id, userId);
    
    // Make sure user is loaded
    if (!ipAsset.user) {
      throw new NotFoundException('User not found for this IP asset');
    }

    const user = ipAsset.user;

    if (ipAsset.status === 'published') {
      return ipAsset;
    }

    if (!user.externalServicesEnabled) {
      throw new BadRequestException('User has not enabled external services');
    }

    try {
      // Read file from disk
      const uploadDir = this.configService.get<string>('app.uploadDir') || './uploads';
      const filePath = path.join(uploadDir, ipAsset.filePath);
      const fileBuffer = await fs.readFile(filePath);

      // Create a file-like object
      const file = {
        buffer: fileBuffer,
        originalname: ipAsset.fileName,
        mimetype: ipAsset.fileType,
        size: ipAsset.fileSize,
        fieldname: 'file',
        encoding: '7bit',
      } as any;

      const externalData = await this.publishToExternalServices(
        file,
        ipAsset.title,
        user
      );

      // Update the IP asset with external data
      ipAsset.arweaveId = externalData.arweaveId;
      ipAsset.ipAssetId = externalData.ipAssetId;
      ipAsset.transactionHash = externalData.txHash;
      ipAsset.status = 'published';
      ipAsset.publishedAt = new Date();

      return this.ipAssetsRepository.save(ipAsset);
    } catch (error) {
      ipAsset.status = 'failed';
      await this.ipAssetsRepository.save(ipAsset);
      throw error;
    }
  }

  private validateFile(file: Express.Multer.File): void {
    const maxFileSize = this.configService.get<number>('app.maxFileSize') || 10485760;
    
    if (file.size > maxFileSize) {
      throw new BadRequestException(
        `File size exceeds maximum limit of ${maxFileSize / 1024 / 1024}MB`
      );
    }

    // Add more validations as needed (file types, etc.)
    const allowedMimeTypes = [
      'image/jpeg', 'image/png', 'image/gif', 'image/webp',
      'application/pdf',
      'video/mp4', 'video/webm',
      'audio/mpeg', 'audio/wav',
    ];

    if (!allowedMimeTypes.includes(file.mimetype)) {
      throw new BadRequestException('File type not allowed');
    }
  }

  private async publishToExternalServices(
    file: any,
    title: string,
    user: User,
  ): Promise<{ arweaveId: string; ipAssetId: string; txHash: string }> {
    // Upload to Arweave
    const arweaveId = await this.arweaveService.uploadFile(file);
    
    // Register with Story Protocol
    const storyResult = await this.storyProtocolService.registerIPAsset(
      arweaveId,
      title,
      user.walletAddress || '0x0000000000000000000000000000000000000000', // Fallback
    );

    return {
      arweaveId,
      ipAssetId: storyResult.ipId,
      txHash: storyResult.txHash,
    };
  }
}