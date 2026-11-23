# What backs genhttp.dev up: SQLite to take the databases while the server
# writes them, restic to keep them deduplicated and encrypted, rclone to speak
# WebDAV to the Nextcloud they are kept in.
#
# Not the server's image: that one is chiseled and has no shell, and nothing
# of this belongs beside the lambdas.
FROM alpine:3.22

RUN apk add --no-cache sqlite restic rclone ca-certificates tzdata

# The server's user, so the databases are opened as the user who owns them and
# nothing it touches changes hands. The folders exist here so the volumes
# mounted on them start out as this user's; rclone's configuration is empty,
# the remote comes from the environment.
RUN addgroup -g 1001 lambda \
 && adduser -D -u 1001 -G lambda -h /home/lambda lambda \
 && mkdir -p /cache /staging /restore /home/lambda/.config/rclone \
 && touch /home/lambda/.config/rclone/rclone.conf \
 && chown -R lambda:lambda /cache /staging /restore /home/lambda

COPY --chmod=755 run.sh /usr/local/bin/backup

USER 1001:1001
ENV HOME=/home/lambda \
    RESTIC_CACHE_DIR=/cache

ENTRYPOINT ["/usr/local/bin/backup"]
CMD ["backup"]
