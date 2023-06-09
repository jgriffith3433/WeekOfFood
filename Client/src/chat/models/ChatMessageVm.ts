
export interface ChatMessageVm {
  from?: string;
  content?: string;
  received: boolean;
  rawContent?: string;
  to?: string;
}
