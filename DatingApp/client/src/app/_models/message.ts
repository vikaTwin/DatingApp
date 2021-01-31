export interface Message {
    id: number;
    senderId: number;
    senderUserName: string;
    senderPhotUrl: string;
    recipientId: number;
    recipientUserName: string;
    recipientPhotoUrl: string;
    content: string;
    dateRead?: Date;
    messageSent: Date;
  }