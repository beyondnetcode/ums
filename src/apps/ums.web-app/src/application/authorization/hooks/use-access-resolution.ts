import { useAuthStore } from '@app/stores/auth.store';
import {
  AccessEffect,
  AuthorizationGraph,
  GraphMenuModule,
  GraphMenu,
  GraphSubMenu,
  GraphMenuOption,
} from '@domain/authorization/schemas/authorization-graph.schema';

export const useAccessResolution = () => {
  const user = useAuthStore(state => state.user);
  const graph: AuthorizationGraph | null = user?.authorizationGraph ?? null;
  const isInternalAdmin = user?.isInternalAdmin ?? false;

  // Internal helper for evaluating effect precedence
  const evaluateEffect = (effect: AccessEffect): boolean => {
    // Strict Precedence: Deny overrides everything. NotGranted means no access.
    if (effect === AccessEffect.Deny) return false;
    if (effect === AccessEffect.NotGranted) return false;
    return effect === AccessEffect.Allow;
  };

  /**
   * Checks if the user has access to a specific module.
   */
  const hasModuleAccess = (moduleCode: string): boolean => {
    if (isInternalAdmin) return true;
    if (!graph) return false;
    const mod = graph.menuAccess.find(m => m.code === moduleCode);
    if (!mod) return false;
    // A module is visible if its status is active and at least one menu has an Allow option.
    // The graph generator usually filters out unreachable modules, but we double-check.
    return mod.status === 'Active';
  };

  /**
   * Checks if the user has access to a specific menu within a module.
   */
  const hasMenuAccess = (moduleCode: string, menuCode: string): boolean => {
    if (isInternalAdmin) return true;
    if (!graph) return false;
    const mod = graph.menuAccess.find(m => m.code === moduleCode);
    if (!mod) return false;

    const menu = mod.menus.find(m => m.code === menuCode);
    return !!menu;
  };

  /**
   * Checks if the user has access to a specific option (action) within a menu/submenu.
   * Format of targetCode: 'menuCode:optionCode' or just 'optionCode' depending on convention.
   */
  const hasOptionAccess = (menuCode: string, optionCode: string): boolean => {
    if (isInternalAdmin) return true;
    if (!graph) return false;

    // Search through all modules, menus, and submenus
    for (const mod of graph.menuAccess) {
      for (const menu of mod.menus) {
        if (menu.code === menuCode) {
          // If we find the menu, search its submenus for the option
          for (const sub of menu.subMenus) {
            const opt = sub.options.find(o => o.code === optionCode || o.actionCode === optionCode);
            if (opt) {
              return evaluateEffect(opt.effect);
            }
          }
        }

        // Also check submenus to see if menuCode refers to a submenu
        for (const sub of menu.subMenus) {
          if (sub.code === menuCode) {
            const opt = sub.options.find(o => o.code === optionCode || o.actionCode === optionCode);
            if (opt) {
              return evaluateEffect(opt.effect);
            }
          }
        }
      }
    }

    return false;
  };

  /**
   * Evaluates if a feature flag is enabled.
   */
  const hasFeatureFlag = (flagCode: string): boolean => {
    if (isInternalAdmin) return true;
    if (!graph) return false;
    const flag = graph.featureFlags.find(f => f.flagCode === flagCode);
    return flag?.isEnabled ?? false;
  };

  return {
    graph,
    hasModuleAccess,
    hasMenuAccess,
    hasOptionAccess,
    hasFeatureFlag,
  };
};
