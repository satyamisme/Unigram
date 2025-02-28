﻿//
// Copyright Fela Ameghino 2015-2023
//
// Distributed under the GNU General Public License v3.0. (See accompanying
// file LICENSE or copy at https://www.gnu.org/licenses/gpl-3.0.txt)
//
using Telegram.Common;
using Telegram.Services;
using Telegram.Td.Api;

namespace Telegram.Streams
{
    public class ReactionFileSource : DelayedFileSource
    {
        private readonly IClientService _clientService;
        private readonly ReactionType _reaction;

        public ReactionFileSource(IClientService clientService, ReactionType reaction)
            : base(clientService, null)
        {
            _clientService = clientService;
            _reaction = reaction;
        }

        public override long Id => GetHashCode();

        public override async void DownloadFile(object sender, UpdateHandler<File> handler)
        {
            if (_file != null && _file.Local.IsDownloadingCompleted)
            {
                handler(sender, _file);
            }
            else
            {
                if (_file == null)
                {
                    if (_reaction is ReactionTypeEmoji emoji)
                    {
                        var response = await _clientService.SendAsync(new GetEmojiReaction(emoji.Emoji));
                        if (response is EmojiReaction reaction)
                        {
                            _file = reaction.ActivateAnimation.StickerValue;
                        }
                    }
                    else if (_reaction is ReactionTypeCustomEmoji customEmoji)
                    {
                        var response = await _clientService.SendAsync(new GetCustomEmojiStickers(new[] { customEmoji.CustomEmojiId }));
                        if (response is Stickers stickers && stickers.StickersValue.Count == 1)
                        {
                            _file = stickers.StickersValue[0].StickerValue;
                        }
                    }
                }

                if (_file == null)
                {
                    return;
                }
                else if (_file.Local.IsDownloadingCompleted)
                {
                    handler(sender, _file);
                    return;
                }

                UpdateManager.Subscribe(sender, _clientService, _file, ref _fileToken, handler, true);

                if (_file.Local.CanBeDownloaded && !_file.Local.IsDownloadingActive)
                {
                    _clientService.DownloadFile(_file.Id, 16);
                }
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is CustomEmojiFileSource y)
            {
                return y.Id == Id;
            }

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _reaction switch
            {
                ReactionTypeEmoji emoji => emoji.Emoji.GetHashCode(),
                ReactionTypeCustomEmoji customEmoji => customEmoji.CustomEmojiId.GetHashCode(),
                _ => base.GetHashCode()
            };
        }
    }
}
