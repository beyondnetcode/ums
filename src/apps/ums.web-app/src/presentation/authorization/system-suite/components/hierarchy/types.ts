export interface AddMenuState {
  code: string;
  label: string;
  desc: string;
  sort: string;
  error: string;
}

export interface AddSubState {
  code: string;
  label: string;
  desc: string;
  sort: string;
  error: string;
}

export interface AddOptState {
  code: string;
  label: string;
  desc: string;
  actionCode: string;
  sort: string;
  error: string;
}

export const emptyMenu = (): AddMenuState => ({
  code: '',
  label: '',
  desc: '',
  sort: '1',
  error: '',
});
export const emptySub = (): AddSubState => ({
  code: '',
  label: '',
  desc: '',
  sort: '1',
  error: '',
});
export const emptyOpt = (): AddOptState => ({
  code: '',
  label: '',
  desc: '',
  actionCode: '',
  sort: '1',
  error: '',
});
