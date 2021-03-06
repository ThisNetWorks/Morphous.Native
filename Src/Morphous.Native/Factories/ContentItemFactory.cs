﻿using Morphous.Native.DTOs;
using Morphous.Native.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Morphous.Native.Factories
{
    public interface IContentItemFactory
    {
        IContentItem Create(ContentItemDto contentItemDto);
    }

    public class ContentItemFactory : IContentItemFactory
    {
        // ContentItem
        public IContentItem Create(ContentItemDto contentItemDto)
        {
            var contentItem = new ContentItem();
            contentItem.Id = contentItemDto.Id;
            contentItem.ContentType = contentItemDto.ContentType;
            contentItem.DisplayType = contentItemDto.DisplayType;
            
            foreach (var zoneDto in contentItemDto.Zones)
            {
                var zone = CreateZone(zoneDto, contentItem);
                contentItem.Zones.Add(zone);
            }
            
            contentItem.Alternates.Add($"ContentItem_{contentItem.Id}");
            contentItem.Alternates.Add($"ContentItem_{contentItem.ContentType}_{contentItem.DisplayType}");
            contentItem.Alternates.Add($"ContentItem_{contentItem.DisplayType}");
            contentItem.Alternates.Add($"ContentItem_{contentItem.ContentType}");
            contentItem.Alternates.Add($"ContentItem");

            return contentItem;
        }

        // Zone
        private ContentZone CreateZone(ZoneDto zoneDto, IContentItem parentItem)
        {
            var zone = new ContentZone();
            zone.Name = zoneDto.Name;
            zone.ContentItem = parentItem;

            foreach (var elementDto in zoneDto.Elements)
            {
                if (elementDto != null)
                {
                    var element = CreateElement(elementDto, zone, parentItem);
                    zone.Elements.Add(element);
                }
            }

            return zone;
        }

        // Elements
        private ContentElement CreateElement(ContentElementDto elementDto, IContentZone parentZone, IContentItem parentItem)
        {
            ContentElement element;

            if (elementDto is ContentPartDto)
            {
                element = CreatePart(elementDto as ContentPartDto);
            }
            else if (elementDto is ContentFieldDto)
            {
                element = CreateField(elementDto as ContentFieldDto);
            }
            else
            {
                element = new ContentElement();
            }

            if (element.Type == null)
            {
                element.Type = elementDto.Type;
            }

            element.Zone = parentZone;

            
            element.Alternates.Add($"{element.Type}_{element.Zone.ContentItem.Id}");
            element.Alternates.Add($"{element.Type}_{parentItem.ContentType}_{parentItem.DisplayType}_{parentZone.Name}");
            element.Alternates.Add($"{element.Type}_{parentItem.ContentType}_{parentItem.DisplayType}");
            element.Alternates.Add($"{element.Type}_{parentItem.DisplayType}_{parentZone.Name}");
            element.Alternates.Add($"{element.Type}_{parentItem.DisplayType}");
            element.Alternates.Add($"{element.Type}_{parentItem.ContentType}");
            element.Alternates.Add($"{element.Type}_{parentZone.Name}");
            element.Alternates.Add($"{element.Type}");

            return element;
        }

        // Parts
        private ContentElement CreatePart(ContentPartDto contentPartDto)
        {
            ContentPart contentPart;

            if (contentPartDto is CommonPartDto)
            {
                contentPart = CreateCommonPart(contentPartDto as CommonPartDto);
            }
            else if (contentPartDto is TitlePartDto)
            {
                var titlePartDto = contentPartDto as TitlePartDto;
                contentPart = new TitlePart { Title = titlePartDto.Title };
            }
            else if (contentPartDto is BodyPartDto)
            {
                var bodyPartDto = contentPartDto as BodyPartDto;
                contentPart = new BodyPart { Html = bodyPartDto.Html};
            }
            else if (contentPartDto is TermPartDto)
            {
                contentPart = CreateTermPart(contentPartDto as TermPartDto);
            }
            else if (contentPartDto is TaxonomyPartDto)
            {
                contentPart = CreateTaxonomyPart(contentPartDto as TaxonomyPartDto);
            }
            else if (contentPartDto is ImagePartDto)
            {
                var imagePartDto = contentPartDto as ImagePartDto;
                contentPart = new ImagePart { Url = imagePartDto.Url, AlternateText = imagePartDto.AlternateText, Width = imagePartDto.Width, Height = imagePartDto.Height };
            }
            else
            {
                throw new NotSupportedException("No mapping from element dto " + contentPartDto.Type + " to element model.");
            }

            return contentPart;
        }

        private ContentPart CreateCommonPart(CommonPartDto commonPartDto)
        {
            var commonPart = new CommonPart();
            commonPart.Id = commonPartDto.Id;
            commonPart.ResourceUrl = commonPartDto.ResourceUrl;
            commonPart.CreatedDate = DateTime.Parse(commonPartDto.CreatedUtc);
            commonPart.PublishedDate = DateTime.Parse(commonPartDto.PublishedUtc);

            return commonPart;
        }

        private ContentPart CreateTermPart(TermPartDto termPartDto)
        {
            var termPart = new TermPart();

            foreach (var contentItemDto in termPartDto.ContentItems)
            {
                termPart.ContentItems.Add(Create(contentItemDto));
            }

            return termPart;
        }

        private ContentPart CreateTaxonomyPart(TaxonomyPartDto taxonomyPartDto)
        {
            var taxonomyPart = new TaxonomyPart();
            taxonomyPart.Terms = CreateTaxonomyItems(taxonomyPartDto.Terms);
            
            return taxonomyPart;
        }

        private IList<ITaxonomyItem> CreateTaxonomyItems(IList<TaxonomyItemDto> itemDtos)
        {
            var items = new List<ITaxonomyItem>();

            foreach (var itemDto in itemDtos)
            {
                var taxonomyItem = new TaxonomyItem();
                taxonomyItem.Id = itemDto.Id;
                taxonomyItem.Title = itemDto.Title;
                taxonomyItem.DisplayUrl = itemDto.DisplayUrl;
                taxonomyItem.Terms = CreateTaxonomyItems(itemDto.Terms);

                items.Add(taxonomyItem);
            }

            return items;
        }

        // Fields
        private ContentElement CreateField(ContentFieldDto contentFieldDto)
        {
            ContentField contentField;

            if (contentFieldDto is BooleanFieldDto)
            {
                var booleanFieldDto = contentFieldDto as BooleanFieldDto;
                contentField = new BooleanField { Value = booleanFieldDto.Value };
            }
            else if (contentFieldDto is MediaFieldDto)
            {
                var mediaFieldDto = contentFieldDto as MediaFieldDto;

                if (mediaFieldDto.Media.Count > 1)
                {
                    contentField = CreateMultiMediaField(mediaFieldDto);
                }
                else
                {
                    contentField = CreateMediaField(mediaFieldDto);
                }
            }
            else
            {
                throw new NotSupportedException("No mapping from element dto " + contentFieldDto.Type + " to element model.");
            }

            contentField.Name = contentFieldDto.Name;

            return contentField;
        }

        private ContentField CreateMediaField(MediaFieldDto mediaFieldDto)
        {
            var mediaField = new MediaField();
            mediaField.Type = nameof(MediaField);

            var mediaItemDto = mediaFieldDto.Media.FirstOrDefault();

            if (mediaItemDto != null)
            {
                mediaField.Media = Create(mediaItemDto);
            }

            return mediaField;
        }

        private ContentField CreateMultiMediaField(MediaFieldDto mediaFieldDto)
        {
            var multiMediaField = new MultiMediaField();

            foreach (var mediaItemDto in mediaFieldDto.Media)
            {
                multiMediaField.Media.Add(Create(mediaItemDto));
            }

            return multiMediaField;
        }
    }
}
 