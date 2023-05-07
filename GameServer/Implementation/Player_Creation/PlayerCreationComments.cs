﻿using GameServer.Models.PlayerData.PlayerCreations;
using GameServer.Models.Request;
using GameServer.Models.Response;
using GameServer.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using GameServer.Utils;
using GameServer.Models.PlayerData;
using System.Linq;
using System;

namespace GameServer.Implementation.Player_Creation
{
    public class PlayerCreationComments
    {
        public static string ListComments(Database database, string username, int page, int per_page, SortColumn sort_column, SortOrder sort_order, int limit, Platform platform, string PlayerCreationIDFilter, string AuthorIDFilter)
        {
            var Comments = new List<PlayerCreationCommentData> { };
            var requestedBy = database.Users.FirstOrDefault(match => match.Username == username);

            foreach (string id in PlayerCreationIDFilter.Split(','))
            {
                Comments = database.PlayerCreationComments.Where(match => match.PlayerCreationId == int.Parse(id)).ToList();
            }

            //sorting
            if (sort_column == SortColumn.created_at)
                Comments.Sort((curr, prev) => prev.CreatedAt.CompareTo(curr.CreatedAt));

            var CommentsList = new List<player_creation_comment> { };

            //calculating pages
            int pageEnd = PageCalculator.GetPageEnd(page, per_page);
            int pageStart = PageCalculator.GetPageStart(page, per_page);
            int totalPages = PageCalculator.GetTotalPages(per_page, Comments.Count);

            if (pageEnd > Comments.Count)
                pageEnd = Comments.Count;

            for (int i = pageStart; i < pageEnd; i++)
            {
                var Comment = Comments[i];
                if (Comment != null)
                {
                    CommentsList.Add(new player_creation_comment
                    {
                        body = Comment.Body,
                        created_at = Comment.CreatedAt.ToString("yyyy-MM-ddThh:mm:sszzz"),
                        updated_at = Comment.UpdatedAt.ToString("yyyy-MM-ddThh:mm:sszzz"),
                        id = Comment.Id,
                        platform = Comment.Platform.ToString(),
                        player_creation_id = Comment.PlayerCreationId,
                        player_id = Comment.PlayerId,
                        username = Comment.Username,
                        rating_down = Comment.RatingDown,
                        rating_up = Comment.RatingUp,
                        rated_by_me = Comment.IsRatedByMe(requestedBy.UserId)
                    });
                }
            }

            var resp = new Response<List<player_creation_comments>>
            {
                status = new ResponseStatus { id = 0, message = "Successful completion" },
                response = new List<player_creation_comments> { new player_creation_comments {
                    page = page,
                    row_start = pageStart,
                    row_end = pageEnd,
                    total = Comments.Count,
                    total_pages = totalPages,
                    PlayerCreationCommentList = CommentsList
                } }
            };

            return resp.Serialize();
        }

        public static string CreateComment(Database database, string username, PlayerCreationComment player_creation_comment)
        {
            var author = database.Users.FirstOrDefault(match => match.Username == username);
            var Creation = database.PlayerCreations.FirstOrDefault(match => match.PlayerCreationId == player_creation_comment.player_creation_id);

            if (author == null || Creation == null)
            {
                var errorResp = new Response<EmptyResponse>
                {
                    status = new ResponseStatus { id = -130, message = "The player doesn't exist" },
                    response = new EmptyResponse { }
                };
                return errorResp.Serialize();
            }

            database.PlayerCreationComments.Add(new PlayerCreationCommentData
            {
                PlayerId = author.UserId,
                Body = player_creation_comment.body,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Platform = Platform.PS3,
                PlayerCreationId = player_creation_comment.player_creation_id
            });
            database.SaveChanges();

            var resp = new Response<EmptyResponse>
            {
                status = new ResponseStatus { id = 0, message = "Successful completion" },
                response = new EmptyResponse { }
            };
            return resp.Serialize();
        }

        public static string DeleteComment(Database database, string username, int id)
        {
            var user = database.Users.FirstOrDefault(match => match.Username == username);
            var comment = database.PlayerCreationComments.FirstOrDefault(match => match.Id == id);

            if (user == null || comment == null)
            {
                var errorResp = new Response<EmptyResponse>
                {
                    status = new ResponseStatus { id = -130, message = "The player doesn't exist" },
                    response = new EmptyResponse { }
                };
                return errorResp.Serialize();
            }

            var creation = database.PlayerCreations.FirstOrDefault(match => match.PlayerCreationId == comment.PlayerCreationId);

            if (creation != null)
            {
                if (creation.PlayerId != user.UserId && comment.PlayerId != user.UserId)
                {
                    var errorResp = new Response<EmptyResponse>
                    {
                        status = new ResponseStatus { id = -130, message = "The player doesn't exist" },
                        response = new EmptyResponse { }
                    };
                    return errorResp.Serialize();
                }
            }

            database.PlayerCreationComments.Remove(comment);
            database.SaveChanges();

            var resp = new Response<EmptyResponse>
            {
                status = new ResponseStatus { id = 0, message = "Successful completion" },
                response = new EmptyResponse { }
            };
            return resp.Serialize();
        }

        public static string RateComment(Database database, string username, PlayerCreationCommentRating player_creation_comment_rating)
        {
            var user = database.Users.FirstOrDefault(match => match.Username == username);
            var comment = database.PlayerCreationComments.FirstOrDefault(match => match.Id == player_creation_comment_rating.player_creation_comment_id);

            if (user == null || comment == null)
            {
                var errorResp = new Response<EmptyResponse>
                {
                    status = new ResponseStatus { id = -130, message = "The player doesn't exist" },
                    response = new EmptyResponse { }
                };
                return errorResp.Serialize();
            }

            var rating = database.PlayerCreationCommentRatings.FirstOrDefault(match =>
                match.PlayerCreationCommentId == player_creation_comment_rating.player_creation_comment_id && match.PlayerId == user.UserId);

            if (rating == null)
            {
                database.PlayerCreationCommentRatings.Add(new PlayerCreationCommentRatingData
                {
                    PlayerCreationCommentId = player_creation_comment_rating.player_creation_comment_id,
                    PlayerId = user.UserId,
                    Type = RatingType.YAY,
                    RatedAt = DateTime.UtcNow
                });
                database.SaveChanges();
            }

            var resp = new Response<EmptyResponse>
            {
                status = new ResponseStatus { id = 0, message = "Successful completion" },
                response = new EmptyResponse { }
            };
            return resp.Serialize();
        }
    }
}