import { useState, useCallback } from 'react';
import { z } from 'zod';

export function useFormValidation<T extends z.ZodTypeAny>(schema: T) {
  const [errors, setErrors] = useState<Record<string, string>>({});

  const validate = useCallback(
    (data: unknown): z.infer<T> | null => {
      setErrors({});
      const result = schema.safeParse(data);
      if (!result.success) {
        const fieldErrors: Record<string, string> = {};
        const flattened = result.error.flatten();
        
        for (const [key, messages] of Object.entries(flattened.fieldErrors)) {
          if (messages && messages.length > 0) {
            fieldErrors[key] = messages[0];
          }
        }
        
        setErrors(fieldErrors);
        return null;
      }
      return result.data;
    },
    [schema]
  );

  const clearErrors = useCallback(() => {
    setErrors({});
  }, []);

  return { errors, validate, clearErrors, setErrors };
}
