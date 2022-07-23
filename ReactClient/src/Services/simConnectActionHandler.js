import { simConnectPost } from './simConnectPost';

export const simActions = {
    Encoder: {
        upperIncrease: () => simConnectPost({action: 'UPPER_ENCODER_INC', actionValue: 1, actionType: 'EncoderAction'}),
        upperDecrease: () => simConnectPost({action: 'UPPER_ENCODER_DEC', actionValue: 1, actionType: 'EncoderAction'}),
        lowerIncrease: () => simConnectPost({action: 'LOWER_ENCODER_INC', actionValue: 1, actionType: 'EncoderAction'}),
        lowerDecrease: () => simConnectPost({action: 'LOWER_ENCODER_DEC', actionValue: 1, actionType: 'EncoderAction'}),
        push: () => simConnectPost({action: 'ENCODER_PUSH', actionValue: 1, actionType: 'EncoderAction'}),
    },

    SimRate : {
        increase: () => simConnectPost({action: 'SIM_RATE_INCR', actionValue: 1, actionType: 'SimEventId'}),
        decrease: () => simConnectPost({action: 'SIM_RATE_DECR', actionValue: 1, actionType: 'SimEventId'}),
    }
}