-- UMS - Add RowVersion to PromotionImpactAnalyses for optimistic concurrency
-- Date: 2026-05-27
-- Scope: SQL Server 2022

ALTER TABLE [ums_iga].[PromotionImpactAnalyses]
ADD [RowVersion] ROWVERSION NOT NULL;
