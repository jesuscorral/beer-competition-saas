# Keycloak Configuration

This directory contains the Keycloak realm export file for the Beer Competition Platform.

## realm-export.json

The `realm-export.json` file contains the complete Keycloak realm configuration including:
- Realm settings
- Clients (bff-api, competition-service, judging-service, frontend-spa)
- Roles (organizer, judge, entrant, steward)
- Test users

### Test Users

The realm export includes four test users for development:

| Username                  | Roles              | Tenant ID                            |
|---------------------------|--------------------|--------------------------------------|
| organizer@beercomp.local  | organizer          | 11111111-1111-1111-1111-111111111111 |
| judge@beercomp.local      | judge, entrant     | 11111111-1111-1111-1111-111111111111 |
| entrant@beercomp.local    | entrant            | 11111111-1111-1111-1111-111111111111 |
| steward@beercomp.local    | steward            | 11111111-1111-1111-1111-111111111111 |

**IMPORTANT**: Passwords are set to `<your-password>` placeholders in the export file for security. 

### Setting User Passwords

Before importing the realm, you need to set actual passwords for the test users. You have two options:

#### Option 1: Manual Configuration (After Import)
1. Import the realm: `docker exec -it keycloak /opt/keycloak/bin/kc.sh import --file /opt/keycloak/data/import/realm-export.json`
2. Access Keycloak admin console: http://localhost:8080
3. Navigate to: Realm "beercomp" → Users
4. For each user, set a password:
   - Click on the user
   - Go to "Credentials" tab
   - Click "Set password"
   - Enter password (e.g., `organizer123` for development)
   - Disable "Temporary" option
   - Click "Save"

#### Option 2: Pre-configured Realm Export (Development Only)
For local development, you can temporarily set passwords in the realm-export.json before importing:

Replace `"value": "<your-password>"` with actual passwords (e.g., `organizer123`, `judge123`, etc.).

**WARNING**: NEVER commit actual passwords to the repository. This is only for local development.

### Importing the Realm

The realm is automatically imported when starting Keycloak via docker-compose:

```bash
docker-compose up -d keycloak
```

The realm export file is mounted at `/opt/keycloak/data/import/realm-export.json` inside the container.

### Exporting the Realm

To export the current realm configuration (after making changes):

```bash
docker exec -it keycloak /opt/keycloak/bin/kc.sh export \
  --dir /opt/keycloak/data/export \
  --realm beercomp \
  --users realm_file
```

Then copy the exported file from the container:

```bash
docker cp keycloak:/opt/keycloak/data/export/beercomp-realm.json infrastructure/keycloak/realm-export.json
```

**Remember**: Replace any passwords with `<your-password>` placeholders before committing!

## Security Notes

- All passwords in committed files MUST use placeholders like `<your-password>`
- Client secrets in `appsettings.json` MUST use placeholders or environment variables
- Production deployments MUST use Azure Key Vault or similar secret management
- NEVER commit real credentials to the repository

For more details, see: `docs/setup/KEYCLOAK_SETUP.md`
