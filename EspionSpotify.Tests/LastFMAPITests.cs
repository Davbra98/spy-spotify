﻿using EspionSpotify.Enums;
using EspionSpotify.MediaTags;
using EspionSpotify.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace EspionSpotify.Tests
{
    public class LastFMAPITests
    {
        private readonly ILastFMAPI _lastFMAPI;

        public LastFMAPITests()
        {
            _lastFMAPI = new LastFMAPI();
        }

        [Fact]
        internal async void TestAPIKeys_ReturnsOk()
        {
            var artist = "artist";
            var title = "title";
            foreach(var key in _lastFMAPI.ApiKeys)
            {
                var request = WebRequest.Create($"http://ws.audioscrobbler.com/2.0/?method=track.getInfo&api_key={key}&artist={artist}&track={title}");
                using (var response = await request.GetResponseAsync())
                {
                    var httpResponse = response as HttpWebResponse;
                    Assert.Equal(HttpStatusCode.OK, httpResponse.StatusCode);
                    response.Dispose();
                }
            }
        }

        [Fact]
        internal void MapLastFMAPITrackToTrack_ReturnsExpectedDetailledTrack()
        {
            var track = new Track() { Artist = "Artist", Title = "Title" };

            var trackExtra = new LastFMTrack()
            {
                Name = "Updated Title",
                Artist = new Artist { Name = "Updated Artist" },
                Album = new Album()
                {
                    Title = "Album Title",
                    Position = "5",
                    Image = new List<Image>() {
                        new Image() { Size = AlbumCoverSize.extralarge.ToString(), Url = "http://xlarge-cover-url.local" },
                        new Image() { Size = AlbumCoverSize.large.ToString(), Url = "http://large-cover-url.local" },
                        new Image() { Size = AlbumCoverSize.medium.ToString(), Url = "http://medium-cover-url.local" },
                        new Image() { Size = AlbumCoverSize.small.ToString(), Url = "http://small-cover-url.local" },
                    }
                },
                Toptags = new Toptags()
                {
                    Tag = new List<Tag>()
                    {
                        new Tag() { Name = "Reggae", Url = "http://reggae-tag.local" },
                        new Tag() { Name = "Rock", Url = "http://rock-tag.local" },
                        new Tag() { Name = "Jazz", Url = "http://jazz-tag.local" },
                    }
                },
                Duration = 1337000,
            };

            _lastFMAPI.MapLastFMTrackToTrack(track, trackExtra);

            Assert.Equal("Updated Title", track.Title);
            Assert.Equal("Updated Artist", track.Artist);
            Assert.Equal("Album Title", track.Album);
            Assert.Equal(5, track.AlbumPosition);
            Assert.Equal(new[] { "Reggae", "Rock", "Jazz" }, track.Genres);
            Assert.Equal(1337, track.Length);
            Assert.Equal("http://xlarge-cover-url.local", track.ArtExtraLargeUrl);
            Assert.Equal("http://large-cover-url.local", track.ArtLargeUrl);
            Assert.Equal("http://medium-cover-url.local", track.ArtMediumUrl);
            Assert.Equal("http://small-cover-url.local", track.ArtSmallUrl);
        }

        [Fact]
        internal void MapLastFMAPTrackToTrack_ReturnsExpectedMissingInfoTrack()
        {
            var track = new Track() { Artist = "Artist", Title = "Title" };

            var trackExtra = new LastFMTrack()
            {
                Artist = new Artist(),
                Album = new Album()
                {
                    Image = new List<Image>() {
                        null,
                        null,
                        null,
                        null,
                    }
                },
                Toptags = new Toptags()
                {
                    Tag = new List<Tag>()
                    {
                        null,
                        null,
                        null,
                    }
                },
            };

            _lastFMAPI.MapLastFMTrackToTrack(track, trackExtra);

            Assert.Equal("Title", track.Title);
            Assert.Equal("Artist", track.Artist);
            Assert.Null(track.Album);
            Assert.Null(track.AlbumPosition);
            Assert.Equal(new string[] { }, track.Genres);
            Assert.Null(track.Length);
            Assert.Null(track.ArtExtraLargeUrl);
            Assert.Null(track.ArtLargeUrl);
            Assert.Null(track.ArtMediumUrl);
            Assert.Null(track.ArtSmallUrl);
        }
    }
}
