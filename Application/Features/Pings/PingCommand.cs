﻿using System.Text.Json.Serialization;
using ZeroReflection.Mediator;

namespace Application.Features.Pings;

[JsonConverter(typeof(JsonStringEnumConverter))]
[System.ComponentModel.Description("MessageType")]
public enum MessageType
{
    Ping = 1 ,
    Pong = 2
}

public class PingCommandChild
{
    public required string ChildMessage { get; set; }
    public MessageType ChildMessageType { get; set; }
}

public class PingCommand : IRequest<string>
{
    public required string Message { get; set; }
    public MessageType MessageType { get; set; }
    
    public required PingCommandChild PingCommandChild { get; set; }
}

public class PingCommandValidator : IValidator<PingCommand>
{
    public void Validate(PingCommand request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            throw new ArgumentNullException(nameof(request.Message));
    }
}

public class PingCommandHandler : IRequestHandler<PingCommand, string>
{
    public Task<string> Handle(PingCommand request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"MessageType: {request.MessageType}");
        return Task.FromResult($"Pong: {request.Message}");
    }
}
