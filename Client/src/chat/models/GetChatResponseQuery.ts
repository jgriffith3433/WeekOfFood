import { ChatMessageVm } from './ChatMessageVm';

export interface GetChatResponseQuery {
    previousMessages?: ChatMessageVm[];
    chatMessage?: ChatMessageVm;
    chatConversationId?: number | undefined;
    currentUrl?: string;
}
