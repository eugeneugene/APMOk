using Grpc.Net.Client;
using System.Net.Security;

namespace APMOk.Services;

internal interface IGrpcChannelProvider
{
    GrpcChannel GetHttpGrpcChannel();
    GrpcChannel GetHttpsGrpcChannel(RemoteCertificateValidationCallback? remoteCertificateValidationCallback = null);
}
