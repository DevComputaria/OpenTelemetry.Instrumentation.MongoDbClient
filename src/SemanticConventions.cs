namespace OpenTelemetry.Instrumentation
{
    /// <summary>
    /// Constants for semantic attribute names used by OpenTelemetry instrumentation following
    /// the OpenTelemetry semantic conventions.
    /// </summary>
    /// <remarks>
    /// See the OpenTelemetry specification for standard attributes:
    /// https://github.com/open-telemetry/semantic-conventions
    /// </remarks>
    internal static class SemanticConventions
    {
        // General attribute constants
        public const string AttributeServiceName = "service.name";
        public const string AttributeServiceNamespace = "service.namespace";
        public const string AttributeServiceVersion = "service.version";
        public const string AttributeServiceInstanceId = "service.instance.id";

        // Net attributes
        public const string AttributeNetTransport = "net.transport";
        public const string AttributeNetPeerIp = "net.peer.ip";
        public const string AttributeNetPeerName = "net.peer.name";
        public const string AttributeNetPeerPort = "net.peer.port";
        public const string AttributeNetHostIp = "net.host.ip";
        public const string AttributeNetHostName = "net.host.name";
        public const string AttributeNetHostPort = "net.host.port";

        // HTTP attributes
        public const string AttributeHttpMethod = "http.method";
        public const string AttributeHttpUrl = "http.url";
        public const string AttributeHttpTarget = "http.target";
        public const string AttributeHttpScheme = "http.scheme";
        public const string AttributeHttpStatusCode = "http.status_code";
        public const string AttributeHttpStatusText = "http.status_text";
        public const string AttributeHttpFlavor = "http.flavor";
        public const string AttributeHttpUserAgent = "http.user_agent";
        public const string AttributeHttpRequestContentLength = "http.request_content_length";
        public const string AttributeHttpResponseContentLength = "http.response_content_length";
        public const string AttributeHttpRoute = "http.route";
        public const string AttributeHttpClientIp = "http.client_ip";

        // Database attributes
        public const string AttributeDbSystem = "db.system";
        public const string AttributeDbName = "db.name";
        public const string AttributeDbConnection = "db.connection_string";
        public const string AttributeDbUser = "db.user";
        public const string AttributeDbStatement = "db.statement";
        public const string AttributeDbOperation = "db.operation";
        public const string AttributeDbSqlTable = "db.sql.table";
        public const string AttributeDbType = "db.type";
        public const string AttributeDbInstance = "db.instance";
        public const string AttributeDbUrl = "db.url";
        public const string AttributeDbCassandraKeyspace = "db.cassandra.keyspace";
        public const string AttributeDbHBaseNamespace = "db.hbase.namespace";
        public const string AttributeDbMongoDbCollection = "db.mongodb.collection";
        public const string AttributeDbRedisDatabaseIndex = "db.redis.database_index";

        // Exception attributes
        public const string AttributeExceptionType = "exception.type";
        public const string AttributeExceptionMessage = "exception.message";
        public const string AttributeExceptionStacktrace = "exception.stacktrace";

        // Messaging attributes
        public const string AttributeMessagingSystem = "messaging.system";
        public const string AttributeMessagingDestination = "messaging.destination";
        public const string AttributeMessagingDestinationKind = "messaging.destination_kind";
        public const string AttributeMessagingTempDestination = "messaging.temp_destination";
        public const string AttributeMessagingProtocol = "messaging.protocol";
        public const string AttributeMessagingProtocolVersion = "messaging.protocol_version";
        public const string AttributeMessagingUrl = "messaging.url";
        public const string AttributeMessagingMessageId = "messaging.message_id";
        public const string AttributeMessagingConversationId = "messaging.conversation_id";
        public const string AttributeMessagingPayloadSize = "messaging.message_payload_size_bytes";
        public const string AttributeMessagingPayloadCompressedSize = "messaging.message_payload_compressed_size_bytes";
        public const string AttributeMessagingOperation = "messaging.operation";

        // RPC attributes
        public const string AttributeRpcSystem = "rpc.system";
        public const string AttributeRpcService = "rpc.service";
        public const string AttributeRpcMethod = "rpc.method";
        public const string AttributeRpcGrpcStatusCode = "rpc.grpc.status_code";

        // FaaS attributes
        public const string AttributeFaasTrigger = "faas.trigger";
        public const string AttributeFaasExecution = "faas.execution";
        public const string AttributeFaasDocumentCollection = "faas.document.collection";
        public const string AttributeFaasDocumentOperation = "faas.document.operation";
        public const string AttributeFaasDocumentTime = "faas.document.time";
        public const string AttributeFaasDocumentName = "faas.document.name";
        public const string AttributeFaasTime = "faas.time";
        public const string AttributeFaasCron = "faas.cron";

        // Error attributes
        public const string AttributeErrorType = "error.type";
        public const string AttributeErrorMessage = "error.message";
        public const string AttributeErrorStacktrace = "error.stacktrace";
    }
}
