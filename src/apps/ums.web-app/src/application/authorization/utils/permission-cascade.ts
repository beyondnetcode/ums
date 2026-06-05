import { SystemSuite } from '../../domain/system-suite';

/**
 * Returns an array of parent IDs (Module, Menu, SubMenu) from root up to (but not including) the targetId.
 * If the targetId is not found, it returns an empty array.
 */
export function getAscendantIds(suite: SystemSuite, targetId: string): string[] {
  const result: string[] = [];

  for (const module of suite.modules) {
    if (module.id === targetId) return result;

    for (const menu of module.menus) {
      if (menu.id === targetId) {
        result.push(module.id);
        return result;
      }

      for (const subMenu of menu.subMenus) {
        if (subMenu.id === targetId) {
          result.push(module.id, menu.id);
          return result;
        }

        for (const option of subMenu.options) {
          if (option.id === targetId) {
            result.push(module.id, menu.id, subMenu.id);
            return result;
          }
        }
      }
    }
  }

  return [];
}

export function getAscendantsWithTypes(
  suite: SystemSuite,
  targetId: string
): { id: string; type: 'Module' | 'Submodule' | 'Page' | 'Option' }[] {
  const result: { id: string; type: 'Module' | 'Submodule' | 'Page' | 'Option' }[] = [];

  for (const module of suite.modules) {
    if (module.id === targetId) return result;

    for (const menu of module.menus) {
      if (menu.id === targetId) {
        result.push({ id: module.id, type: 'Module' });
        return result;
      }

      for (const subMenu of menu.subMenus) {
        if (subMenu.id === targetId) {
          result.push({ id: module.id, type: 'Module' }, { id: menu.id, type: 'Submodule' });
          return result;
        }

        for (const option of subMenu.options) {
          if (option.id === targetId) {
            result.push(
              { id: module.id, type: 'Module' },
              { id: menu.id, type: 'Submodule' },
              { id: subMenu.id, type: 'Page' }
            );
            return result;
          }
        }
      }
    }
  }

  return [];
}

/**
 * Returns true if the action code implies a read/view operation.
 */
export function isReadAction(actionCode: string): boolean {
  const upper = actionCode.toUpperCase();
  return (
    upper === 'VIEW' ||
    upper === 'LIST' ||
    upper === 'READ' ||
    upper.endsWith('_VIEW') ||
    upper.endsWith('_LIST')
  );
}

/**
 * Given an option ID, if it's a "Write/Manage" option, returns the IDs and types of any "Read/View" options
 * in the same SubMenu, to enforce CRUD logic where write access implies read access.
 */
export function getSiblingViewOptions(
  suite: SystemSuite,
  optionId: string
): { id: string; type: 'Option' }[] {
  const result: { id: string; type: 'Option' }[] = [];

  for (const module of suite.modules) {
    for (const menu of module.menus) {
      for (const subMenu of menu.subMenus) {
        const targetOption = subMenu.options.find(o => o.id === optionId);
        if (targetOption) {
          // If the target option itself is already a read action, there's no implied read to fetch
          if (isReadAction(targetOption.actionCode)) {
            return [];
          }

          // Find all sibling read options
          for (const sibling of subMenu.options) {
            if (sibling.id !== optionId && isReadAction(sibling.actionCode)) {
              result.push({ id: sibling.id, type: 'Option' });
            }
          }
          return result;
        }
      }
    }
  }
  return [];
}
