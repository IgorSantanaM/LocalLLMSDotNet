using Octokit;
using System;
using System.Collections.Generic;
using System.Text;

namespace AiChatClient.Maui.Services
{
    public class GitHubServices(GitHubClient client)
    {
        readonly GitHubClient _client = client;

        public string GetIgorUserName() => "igorsantanam";

        public async Task<string> GetUserBio(string userName)
        {
            var user = await _client.User.Get(userName);
            return user.Bio;
        }

        public async Task<int> GetRepositoryCount(string userName)
        {
            var user = await _client.User.Get(userName);
            return user.PublicRepos;
        }
    }
}
