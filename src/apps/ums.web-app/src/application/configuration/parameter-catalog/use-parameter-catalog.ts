import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { parameterCatalogService } from '@infrastructure/configuration/services/parameter-catalog/parameter-catalog.service';
import type {
  ParameterDefinitionFilter,
  CreateParameterDefinitionPayload,
  UpdateParameterDefinitionPayload,
} from '@domain/configuration/schemas/parameter-catalog/parameter-definition.schema';

export function useParameterDefinitions(filter?: ParameterDefinitionFilter) {
  return useQuery({
    queryKey: ['parameter-definitions', filter],
    queryFn: () => parameterCatalogService.getAll(filter),
    staleTime: 5 * 60 * 1000,
  });
}

export function useParameterDefinitionById(id: string) {
  return useQuery({
    queryKey: ['parameter-definition', id],
    queryFn: () => parameterCatalogService.getById(id),
    enabled: !!id,
  });
}

export function useCreateParameterDefinition() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (payload: CreateParameterDefinitionPayload) =>
      parameterCatalogService.createParameterDefinition(payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['parameter-definitions'] });
    },
  });
}

export function useUpdateParameterDefinition() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdateParameterDefinitionPayload }) =>
      parameterCatalogService.updateParameterDefinition(id, payload),
    onSuccess: data => {
      queryClient.invalidateQueries({ queryKey: ['parameter-definitions'] });
      queryClient.setQueryData(['parameter-definition', data.id], data);
    },
  });
}

export function useDeleteParameterDefinition() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => parameterCatalogService.deleteParameterDefinition(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['parameter-definitions'] });
    },
  });
}
