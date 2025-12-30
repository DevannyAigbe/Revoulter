// src/modules/ip-assets/ip-assets.controller.ts
import { 
  Controller, Get, Post, Body, UploadedFile, 
  UseInterceptors, UseGuards, Req, Param, Patch,
  BadRequestException, NotFoundException 
} from '@nestjs/common';
import { FileInterceptor } from '@nestjs/platform-express';
import { diskStorage } from 'multer';
import { ConfigService } from '@nestjs/config';

import { PrivyAuthGuard } from '../../common/guards/privy-auth.guard';
import { CurrentUser } from '../../common/decorators/current-user.decorator';
import { IpAssetsService } from './ip-assets.service';
import { UsersService } from '../../users/users.service';
import { CreateIPAssetDto } from './dto/create-ip-asset.dto';

@Controller('ip-assets')
@UseGuards(PrivyAuthGuard)
export class IpAssetsController {
  private uploadOptions: any;

  constructor(
    private readonly ipAssetsService: IpAssetsService,
    private readonly usersService: UsersService,
    private configService: ConfigService,
  ) {
    // Initialize upload options in constructor
    this.initializeUploadOptions();
  }

  private initializeUploadOptions() {
    const uploadDir = this.configService.get<string>('app.uploadDir') || './uploads';
    const maxFileSize = this.configService.get<number>('app.maxFileSize') || 10485760;
    
    this.uploadOptions = {
      storage: diskStorage({
        destination: (req: any, file: Express.Multer.File, cb: any) => {
          cb(null, uploadDir);
        },
        filename: (req: any, file: Express.Multer.File, cb: any) => {
          const uniqueSuffix = Date.now() + '-' + Math.round(Math.random() * 1e9);
          cb(null, file.fieldname + '-' + uniqueSuffix);
        },
      }),
      limits: {
        fileSize: maxFileSize,
      },
    };
  }

  @Post()
  @UseInterceptors(FileInterceptor('file', {
    storage: diskStorage({
      destination: (req: any, file: Express.Multer.File, cb: any) => {
        const uploadDir = process.env.UPLOAD_DIR || './uploads';
        cb(null, uploadDir);
      },
      filename: (req: any, file: Express.Multer.File, cb: any) => {
        const uniqueSuffix = Date.now() + '-' + Math.round(Math.random() * 1e9);
        cb(null, file.fieldname + '-' + uniqueSuffix);
      },
    }),
    limits: {
      fileSize: parseInt(process.env.MAX_FILE_SIZE || '10485760', 10),
    },
  }))
  async create(
    @UploadedFile() file: Express.Multer.File,
    @Body() createDto: CreateIPAssetDto,
    @CurrentUser() userClaims: any,
  ) {
    if (!userClaims) {
      throw new BadRequestException('User not authenticated');
    }

    const user = await this.usersService.findOrCreate(userClaims);
    return this.ipAssetsService.create(createDto, file, user);
  }

  @Get()
  async findAll(@CurrentUser() userClaims: any) {
    if (!userClaims) {
      throw new BadRequestException('User not authenticated');
    }

    const user = await this.usersService.findById(userClaims.userId);
    if (!user) {
      throw new NotFoundException('User not found');
    }

    return this.ipAssetsService.findAllByUser(user.privyId);
  }

  @Get(':id')
  async findOne(@Param('id') id: string, @CurrentUser() userClaims: any) {
    if (!userClaims) {
      throw new BadRequestException('User not authenticated');
    }

    const user = await this.usersService.findById(userClaims.userId);
    if (!user) {
      throw new NotFoundException('User not found');
    }

    return this.ipAssetsService.findOne(id, user.privyId);
  }

  @Patch(':id/sync')
  async syncWithExternalServices(@Param('id') id: string, @CurrentUser() userClaims: any) {
    if (!userClaims) {
      throw new BadRequestException('User not authenticated');
    }

    const user = await this.usersService.findById(userClaims.userId);
    if (!user) {
      throw new NotFoundException('User not found');
    }

    return this.ipAssetsService.syncWithExternalServices(id, user.privyId);
  }
}