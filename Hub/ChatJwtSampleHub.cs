﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace SignalRHubs
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize(Roles = "Admin")]
    [Authorize(Policy = "ClaimBasedAuth")]
    [Authorize(Policy = "PolicyBasedAuth")]
    public class ChatJwtSampleHub : ChatBase
    {
    }
}
