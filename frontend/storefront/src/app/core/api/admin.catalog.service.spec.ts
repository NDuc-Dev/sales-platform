import { TestBed } from '@angular/core/testing';

import { AdminCatalogService } from './admin.catalog.service';

describe('AdminCatalogService', () => {
  let service: AdminCatalogService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AdminCatalogService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
