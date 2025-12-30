// src/modules/ip-assets/dto/create-ip-asset.dto.ts
import { IsString, IsOptional, IsBoolean } from 'class-validator';

export class CreateIPAssetDto {
  @IsString()
  title: string;

  @IsOptional()
  @IsBoolean()
  useExternalServices?: boolean;
}