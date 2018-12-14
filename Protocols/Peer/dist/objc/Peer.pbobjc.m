// Generated by the protocol buffer compiler.  DO NOT EDIT!
// source: Peer.proto

// This CPP symbol can be defined to use imports that match up to the framework
// imports needed when using CocoaPods.
#if !defined(GPB_USE_PROTOBUF_FRAMEWORK_IMPORTS)
 #define GPB_USE_PROTOBUF_FRAMEWORK_IMPORTS 0
#endif

#if GPB_USE_PROTOBUF_FRAMEWORK_IMPORTS
 #import <Protobuf/GPBProtocolBuffers_RuntimeSupport.h>
#else
 #import "GPBProtocolBuffers_RuntimeSupport.h"
#endif

#import "Peer.pbobjc.h"
// @@protoc_insertion_point(imports)

#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Wdeprecated-declarations"

#pragma mark - PeerRoot

@implementation PeerRoot

// No extensions in the file and no imports, so no need to generate
// +extensionRegistry.

@end

#pragma mark - PeerRoot_FileDescriptor

static GPBFileDescriptor *PeerRoot_FileDescriptor(void) {
  // This is called by +initialize so there is no need to worry
  // about thread safety of the singleton.
  static GPBFileDescriptor *descriptor = NULL;
  if (!descriptor) {
    GPB_DEBUG_CHECK_RUNTIME_VERSIONS();
    descriptor = [[GPBFileDescriptor alloc] initWithPackage:@"ADL.Protocol.Peer"
                                                     syntax:GPBFileSyntaxProto3];
  }
  return descriptor;
}

#pragma mark - PingRequest

@implementation PingRequest

@dynamic ping;

typedef struct PingRequest__storage_ {
  uint32_t _has_storage_[1];
  NSString *ping;
} PingRequest__storage_;

// This method is threadsafe because it is initially called
// in +initialize for each subclass.
+ (GPBDescriptor *)descriptor {
  static GPBDescriptor *descriptor = nil;
  if (!descriptor) {
    static GPBMessageFieldDescription fields[] = {
      {
        .name = "ping",
        .dataTypeSpecific.className = NULL,
        .number = PingRequest_FieldNumber_Ping,
        .hasIndex = 0,
        .offset = (uint32_t)offsetof(PingRequest__storage_, ping),
        .flags = GPBFieldOptional,
        .dataType = GPBDataTypeString,
      },
    };
    GPBDescriptor *localDescriptor =
        [GPBDescriptor allocDescriptorForClass:[PingRequest class]
                                     rootClass:[PeerRoot class]
                                          file:PeerRoot_FileDescriptor()
                                        fields:fields
                                    fieldCount:(uint32_t)(sizeof(fields) / sizeof(GPBMessageFieldDescription))
                                   storageSize:sizeof(PingRequest__storage_)
                                         flags:GPBDescriptorInitializationFlag_None];
    NSAssert(descriptor == nil, @"Startup recursed!");
    descriptor = localDescriptor;
  }
  return descriptor;
}

@end

#pragma mark - PongResponse

@implementation PongResponse

@dynamic pong;

typedef struct PongResponse__storage_ {
  uint32_t _has_storage_[1];
  NSString *pong;
} PongResponse__storage_;

// This method is threadsafe because it is initially called
// in +initialize for each subclass.
+ (GPBDescriptor *)descriptor {
  static GPBDescriptor *descriptor = nil;
  if (!descriptor) {
    static GPBMessageFieldDescription fields[] = {
      {
        .name = "pong",
        .dataTypeSpecific.className = NULL,
        .number = PongResponse_FieldNumber_Pong,
        .hasIndex = 0,
        .offset = (uint32_t)offsetof(PongResponse__storage_, pong),
        .flags = GPBFieldOptional,
        .dataType = GPBDataTypeString,
      },
    };
    GPBDescriptor *localDescriptor =
        [GPBDescriptor allocDescriptorForClass:[PongResponse class]
                                     rootClass:[PeerRoot class]
                                          file:PeerRoot_FileDescriptor()
                                        fields:fields
                                    fieldCount:(uint32_t)(sizeof(fields) / sizeof(GPBMessageFieldDescription))
                                   storageSize:sizeof(PongResponse__storage_)
                                         flags:GPBDescriptorInitializationFlag_None];
    NSAssert(descriptor == nil, @"Startup recursed!");
    descriptor = localDescriptor;
  }
  return descriptor;
}

@end

#pragma mark - PeerInfoRequest

@implementation PeerInfoRequest

@dynamic ping;

typedef struct PeerInfoRequest__storage_ {
  uint32_t _has_storage_[1];
  NSString *ping;
} PeerInfoRequest__storage_;

// This method is threadsafe because it is initially called
// in +initialize for each subclass.
+ (GPBDescriptor *)descriptor {
  static GPBDescriptor *descriptor = nil;
  if (!descriptor) {
    static GPBMessageFieldDescription fields[] = {
      {
        .name = "ping",
        .dataTypeSpecific.className = NULL,
        .number = PeerInfoRequest_FieldNumber_Ping,
        .hasIndex = 0,
        .offset = (uint32_t)offsetof(PeerInfoRequest__storage_, ping),
        .flags = GPBFieldOptional,
        .dataType = GPBDataTypeString,
      },
    };
    GPBDescriptor *localDescriptor =
        [GPBDescriptor allocDescriptorForClass:[PeerInfoRequest class]
                                     rootClass:[PeerRoot class]
                                          file:PeerRoot_FileDescriptor()
                                        fields:fields
                                    fieldCount:(uint32_t)(sizeof(fields) / sizeof(GPBMessageFieldDescription))
                                   storageSize:sizeof(PeerInfoRequest__storage_)
                                         flags:GPBDescriptorInitializationFlag_None];
    NSAssert(descriptor == nil, @"Startup recursed!");
    descriptor = localDescriptor;
  }
  return descriptor;
}

@end

#pragma mark - PeerInfoResponse

@implementation PeerInfoResponse

@dynamic pong;

typedef struct PeerInfoResponse__storage_ {
  uint32_t _has_storage_[1];
  NSString *pong;
} PeerInfoResponse__storage_;

// This method is threadsafe because it is initially called
// in +initialize for each subclass.
+ (GPBDescriptor *)descriptor {
  static GPBDescriptor *descriptor = nil;
  if (!descriptor) {
    static GPBMessageFieldDescription fields[] = {
      {
        .name = "pong",
        .dataTypeSpecific.className = NULL,
        .number = PeerInfoResponse_FieldNumber_Pong,
        .hasIndex = 0,
        .offset = (uint32_t)offsetof(PeerInfoResponse__storage_, pong),
        .flags = GPBFieldOptional,
        .dataType = GPBDataTypeString,
      },
    };
    GPBDescriptor *localDescriptor =
        [GPBDescriptor allocDescriptorForClass:[PeerInfoResponse class]
                                     rootClass:[PeerRoot class]
                                          file:PeerRoot_FileDescriptor()
                                        fields:fields
                                    fieldCount:(uint32_t)(sizeof(fields) / sizeof(GPBMessageFieldDescription))
                                   storageSize:sizeof(PeerInfoResponse__storage_)
                                         flags:GPBDescriptorInitializationFlag_None];
    NSAssert(descriptor == nil, @"Startup recursed!");
    descriptor = localDescriptor;
  }
  return descriptor;
}

@end

#pragma mark - PeerNeighborsRequest

@implementation PeerNeighborsRequest

@dynamic ping;

typedef struct PeerNeighborsRequest__storage_ {
  uint32_t _has_storage_[1];
  NSString *ping;
} PeerNeighborsRequest__storage_;

// This method is threadsafe because it is initially called
// in +initialize for each subclass.
+ (GPBDescriptor *)descriptor {
  static GPBDescriptor *descriptor = nil;
  if (!descriptor) {
    static GPBMessageFieldDescription fields[] = {
      {
        .name = "ping",
        .dataTypeSpecific.className = NULL,
        .number = PeerNeighborsRequest_FieldNumber_Ping,
        .hasIndex = 0,
        .offset = (uint32_t)offsetof(PeerNeighborsRequest__storage_, ping),
        .flags = GPBFieldOptional,
        .dataType = GPBDataTypeString,
      },
    };
    GPBDescriptor *localDescriptor =
        [GPBDescriptor allocDescriptorForClass:[PeerNeighborsRequest class]
                                     rootClass:[PeerRoot class]
                                          file:PeerRoot_FileDescriptor()
                                        fields:fields
                                    fieldCount:(uint32_t)(sizeof(fields) / sizeof(GPBMessageFieldDescription))
                                   storageSize:sizeof(PeerNeighborsRequest__storage_)
                                         flags:GPBDescriptorInitializationFlag_None];
    NSAssert(descriptor == nil, @"Startup recursed!");
    descriptor = localDescriptor;
  }
  return descriptor;
}

@end

#pragma mark - PeerNeighborsResponse

@implementation PeerNeighborsResponse

@dynamic pong;

typedef struct PeerNeighborsResponse__storage_ {
  uint32_t _has_storage_[1];
  NSString *pong;
} PeerNeighborsResponse__storage_;

// This method is threadsafe because it is initially called
// in +initialize for each subclass.
+ (GPBDescriptor *)descriptor {
  static GPBDescriptor *descriptor = nil;
  if (!descriptor) {
    static GPBMessageFieldDescription fields[] = {
      {
        .name = "pong",
        .dataTypeSpecific.className = NULL,
        .number = PeerNeighborsResponse_FieldNumber_Pong,
        .hasIndex = 0,
        .offset = (uint32_t)offsetof(PeerNeighborsResponse__storage_, pong),
        .flags = GPBFieldOptional,
        .dataType = GPBDataTypeString,
      },
    };
    GPBDescriptor *localDescriptor =
        [GPBDescriptor allocDescriptorForClass:[PeerNeighborsResponse class]
                                     rootClass:[PeerRoot class]
                                          file:PeerRoot_FileDescriptor()
                                        fields:fields
                                    fieldCount:(uint32_t)(sizeof(fields) / sizeof(GPBMessageFieldDescription))
                                   storageSize:sizeof(PeerNeighborsResponse__storage_)
                                         flags:GPBDescriptorInitializationFlag_None];
    NSAssert(descriptor == nil, @"Startup recursed!");
    descriptor = localDescriptor;
  }
  return descriptor;
}

@end

#pragma mark - ChallengeRequest

@implementation ChallengeRequest

@dynamic type;
@dynamic nonce;

typedef struct ChallengeRequest__storage_ {
  uint32_t _has_storage_[1];
  int32_t type;
  int32_t nonce;
} ChallengeRequest__storage_;

// This method is threadsafe because it is initially called
// in +initialize for each subclass.
+ (GPBDescriptor *)descriptor {
  static GPBDescriptor *descriptor = nil;
  if (!descriptor) {
    static GPBMessageFieldDescription fields[] = {
      {
        .name = "type",
        .dataTypeSpecific.className = NULL,
        .number = ChallengeRequest_FieldNumber_Type,
        .hasIndex = 0,
        .offset = (uint32_t)offsetof(ChallengeRequest__storage_, type),
        .flags = GPBFieldOptional,
        .dataType = GPBDataTypeInt32,
      },
      {
        .name = "nonce",
        .dataTypeSpecific.className = NULL,
        .number = ChallengeRequest_FieldNumber_Nonce,
        .hasIndex = 1,
        .offset = (uint32_t)offsetof(ChallengeRequest__storage_, nonce),
        .flags = GPBFieldOptional,
        .dataType = GPBDataTypeInt32,
      },
    };
    GPBDescriptor *localDescriptor =
        [GPBDescriptor allocDescriptorForClass:[ChallengeRequest class]
                                     rootClass:[PeerRoot class]
                                          file:PeerRoot_FileDescriptor()
                                        fields:fields
                                    fieldCount:(uint32_t)(sizeof(fields) / sizeof(GPBMessageFieldDescription))
                                   storageSize:sizeof(ChallengeRequest__storage_)
                                         flags:GPBDescriptorInitializationFlag_None];
    NSAssert(descriptor == nil, @"Startup recursed!");
    descriptor = localDescriptor;
  }
  return descriptor;
}

@end

#pragma mark - ChallengeResponse

@implementation ChallengeResponse

@dynamic type;
@dynamic signedNonce;
@dynamic publicKey;

typedef struct ChallengeResponse__storage_ {
  uint32_t _has_storage_[1];
  int32_t type;
  NSString *signedNonce;
  NSString *publicKey;
} ChallengeResponse__storage_;

// This method is threadsafe because it is initially called
// in +initialize for each subclass.
+ (GPBDescriptor *)descriptor {
  static GPBDescriptor *descriptor = nil;
  if (!descriptor) {
    static GPBMessageFieldDescription fields[] = {
      {
        .name = "type",
        .dataTypeSpecific.className = NULL,
        .number = ChallengeResponse_FieldNumber_Type,
        .hasIndex = 0,
        .offset = (uint32_t)offsetof(ChallengeResponse__storage_, type),
        .flags = GPBFieldOptional,
        .dataType = GPBDataTypeInt32,
      },
      {
        .name = "signedNonce",
        .dataTypeSpecific.className = NULL,
        .number = ChallengeResponse_FieldNumber_SignedNonce,
        .hasIndex = 1,
        .offset = (uint32_t)offsetof(ChallengeResponse__storage_, signedNonce),
        .flags = (GPBFieldFlags)(GPBFieldOptional | GPBFieldTextFormatNameCustom),
        .dataType = GPBDataTypeString,
      },
      {
        .name = "publicKey",
        .dataTypeSpecific.className = NULL,
        .number = ChallengeResponse_FieldNumber_PublicKey,
        .hasIndex = 2,
        .offset = (uint32_t)offsetof(ChallengeResponse__storage_, publicKey),
        .flags = (GPBFieldFlags)(GPBFieldOptional | GPBFieldTextFormatNameCustom),
        .dataType = GPBDataTypeString,
      },
    };
    GPBDescriptor *localDescriptor =
        [GPBDescriptor allocDescriptorForClass:[ChallengeResponse class]
                                     rootClass:[PeerRoot class]
                                          file:PeerRoot_FileDescriptor()
                                        fields:fields
                                    fieldCount:(uint32_t)(sizeof(fields) / sizeof(GPBMessageFieldDescription))
                                   storageSize:sizeof(ChallengeResponse__storage_)
                                         flags:GPBDescriptorInitializationFlag_None];
#if !GPBOBJC_SKIP_MESSAGE_TEXTFORMAT_EXTRAS
    static const char *extraTextFormatInfo =
        "\002\002\013\000\003\t\000";
    [localDescriptor setupExtraTextInfo:extraTextFormatInfo];
#endif  // !GPBOBJC_SKIP_MESSAGE_TEXTFORMAT_EXTRAS
    NSAssert(descriptor == nil, @"Startup recursed!");
    descriptor = localDescriptor;
  }
  return descriptor;
}

@end


#pragma clang diagnostic pop

// @@protoc_insertion_point(global_scope)
