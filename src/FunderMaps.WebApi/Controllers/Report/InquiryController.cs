﻿using AutoMapper;
using FunderMaps.Controllers;
using FunderMaps.Core.Entities;
using FunderMaps.Core.Types;
using FunderMaps.Core.UseCases;
using FunderMaps.Helpers;
using FunderMaps.WebApi.DataTransferObjects;
using FunderMaps.WebApi.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#pragma warning disable CA1062 // Validate arguments of public methods
namespace FunderMaps.WebApi.Controllers.Report
{
    /// <summary>
    ///     Endpoint controller for inquiry operations.
    /// </summary>
    [Route("inquiry")]
    public class InquiryController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly InquiryUseCase _inquiryUseCase;

        /// <summary>
        ///     Create new instance.
        /// </summary>
        public InquiryController(IMapper mapper, InquiryUseCase inquiryUseCase)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _inquiryUseCase = inquiryUseCase ?? throw new ArgumentNullException(nameof(inquiryUseCase));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetAsync(int id)
        {
            var inquiry = await _inquiryUseCase.GetAsync(id);

            return Ok(_mapper.Map<InquiryDto>(inquiry));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] PaginationModel pagination)
        {
            // Act.
            IAsyncEnumerable<Inquiry> inquiryList = _inquiryUseCase.GetAllAsync(pagination.Navigation);

            // Map.
            var result = await _mapper.MapAsync<IList<InquiryDto>, Inquiry>(inquiryList);

            // Return.
            return Ok(result);
        }

        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentAsync([FromQuery] PaginationModel pagination)
        {
            // FUTURE: _inquiryUseCase.GetAllRecentAsync
            // Act.
            IAsyncEnumerable<Inquiry> inquiryList = _inquiryUseCase.GetAllAsync(pagination.Navigation);

            // Map.
            var result = await _mapper.MapAsync<IList<InquiryDto>, Inquiry>(inquiryList);

            // Return.
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] InquiryDto input)
        {
            var inquiry = await _inquiryUseCase.CreateAsync(_mapper.Map<Inquiry>(input));

            return Ok(_mapper.Map<InquiryDto>(inquiry));
        }

        [HttpPost("upload-document")]
        public async Task<IActionResult> UploadDocumentAsync(IFormFile input)
        {
            // TODO: Replace with validator?
            var virtualFile = new ApplicationFileWrapper(input, Constants.AllowedFileMimes);
            if (!virtualFile.IsValid)
            {
                throw new ArgumentException(); // TODO
            }

            // Act.
            var fileName = await _inquiryUseCase.StoreDocumentAsync(
                input.OpenReadStream(),
                input.FileName,
                input.ContentType);

            // Return.
            return Ok(fileName);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] InquiryDto input)
        {
            // Map.
            var inquiry = _mapper.Map<Inquiry>(input);
            inquiry.Id = id;

            // Act.
            await _inquiryUseCase.UpdateAsync(inquiry);

            // Return.
            return NoContent();
        }

        [HttpPost("{id:int}/status_review")]
        public async Task<IActionResult> SetStatusReviewAsync(int id, StatusChangeDto input)
        {
            // Act.
            await _inquiryUseCase.UpdateStatusAsync(id, AuditStatus.PendingReview, input.Message);

            // Return.
            return NoContent();
        }

        [HttpPost("{id:int}/status_rejected")]
        public async Task<IActionResult> SetStatusRejectedAsync(int id, StatusChangeDto input)
        {
            // Act.
            await _inquiryUseCase.UpdateStatusAsync(id, AuditStatus.Rejected, input.Message);

            // Return.
            return NoContent();
        }

        [HttpPost("{id:int}/status_approved")]
        public async Task<IActionResult> SetStatusApprovedAsync(int id, StatusChangeDto input)
        {
            // Act.
            await _inquiryUseCase.UpdateStatusAsync(id, AuditStatus.Done, input.Message);

            // Return.
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            // Act.
            await _inquiryUseCase.DeleteAsync(id);

            // Return.
            return NoContent();
        }
    }
}
#pragma warning restore CA1062 // Validate arguments of public methods
