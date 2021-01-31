using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController : BaseApiController
    {
        private readonly IMessageRepository _messageRepo;
        private readonly IUserRepository _userRepo;
        private readonly IMapper _mapper;
        public MessagesController(IUserRepository userRepository, IMessageRepository messageRepo, IMapper mapper)
        {
            _mapper = mapper;
            _userRepo = userRepository;
            _messageRepo = messageRepo;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var userName = User.GetUserName();
            if (userName == createMessageDto.RecipientUserName.ToLower())
                return BadRequest("You cant send message to yourself");

            var sender = await _userRepo.GetUserByUsernameAsync(userName);
            var recepient = await _userRepo.GetUserByUsernameAsync(createMessageDto.RecipientUserName);
            if (recepient == null)
                return NotFound();

            var message = new Message
            {
                Sender = sender,
                Recipient = recepient,
                SenderUserName = sender.UserName,
                RecipientUserName = recepient.UserName,
                Content = createMessageDto.Content
            };

            _messageRepo.AddMessage(message);

            if (await _messageRepo.SaveAllAsync())
                return Ok(_mapper.Map<MessageDto>(message));
            return BadRequest("Failed to send message");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUserName();
            var messages = await _messageRepo.GetMessagesForUser(messageParams);
            Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages);
            return messages;
        }

        [HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        {
            var currentUsername = User.GetUserName();
            return Ok(await _messageRepo.GetMessageThread(currentUsername, username));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUserName();
            var message = await _messageRepo.GetMessage(id);
            if (message.Sender.UserName != username && message.Recipient.UserName != username)
                return Unauthorized();

            if (message.Sender.UserName == username)
                message.SenderDeleted = true;
            if (message.Recipient.UserName == username)
                message.RecipientDeleted = true;

            if (message.SenderDeleted && message.RecipientDeleted)
                _messageRepo.DeleteMessage(message);

            if (await _messageRepo.SaveAllAsync())
                return Ok();
                
            return BadRequest("Problem deleting the message");
        }
    }
}