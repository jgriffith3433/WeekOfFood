
export interface ChatMessageVm {
  role?: string;
  content?: string;
  received: boolean;
  rawContent?: string;
  name?: string;
}
