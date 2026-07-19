using PracCentral.Infrastructure.Logging;
using PracCentral.Models.Json;
using PracCentral.Services.Engine;
using PracCentral.Services.Storage;
using PracCentral.Services.Threading;
using PracCentral.Services.Validation;

namespace PracCentral.Core;

public sealed record ModuleContext(
    IMainThreadDispatcher MainThreadDispatcher,
    IServerBridge ServerBridge,
    IJsonStorageService JsonStorageService,
    IInputSanitizer InputSanitizer,
    EventSubscriptionRegistry EventSubscriptionRegistry,
    IPracLogger Logger,
    PluginConfig Config);
