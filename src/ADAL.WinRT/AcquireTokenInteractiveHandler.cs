﻿//----------------------------------------------------------------------
// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// Apache License 2.0
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Networking;

namespace Microsoft.IdentityModel.Clients.ActiveDirectory
{
    internal partial class AcquireTokenInteractiveHandler
    {
        protected override async Task PreTokenRequest()
        {
            await base.PreTokenRequest();
            await this.AcquireAuthorizationAsync();
            this.VerifyAuthorizationResult();
        }

        private async Task AcquireAuthorizationAsync()
        {
            Uri authorizationUri = this.CreateAuthorizationUri(await IncludeFormsAuthParamsAsync());
            this.authorizationResult = await webUi.AuthenticateAsync(authorizationUri, this.redirectUri, this.CallState);
        }

        internal static async Task<bool> IncludeFormsAuthParamsAsync()
        {
            return IsDomainJoined() && await IsUserLocalAsync();
        }

        private static bool IsDomainJoined()
        {
            IReadOnlyList<HostName> hostNamesList = Windows.Networking.Connectivity.NetworkInformation
                .GetHostNames();

            foreach (var entry in hostNamesList)
            {
                if (entry.Type == HostNameType.DomainName)
                {
                    return true;
                }
            }

            return false;
        }

        private async static Task<bool> IsUserLocalAsync()
        {
            if (!Windows.System.UserProfile.UserInformation.NameAccessAllowed)
            {
                throw new AdalException(AdalError.CannotAccessUserInformation);
            }

            try
            {
                return string.IsNullOrEmpty(await Windows.System.UserProfile.UserInformation.GetDomainNameAsync());
            }
            catch (UnauthorizedAccessException)
            {
                // This mostly means Enterprise capability is missing, so WIA cannot be used and
                // we return true to add form auth parameter in the caller.
                return true;
            }
        }
    }
}