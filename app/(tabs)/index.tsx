import React, { useState } from "react";
import {
  View,
  Text,
  StyleSheet,
  ScrollView,
  Pressable,
  useColorScheme,
  Platform,
  Dimensions,
} from "react-native";
import { useSafeAreaInsets } from "react-native-safe-area-context";
import { Ionicons, MaterialCommunityIcons } from "@expo/vector-icons";
import { router } from "expo-router";
import Animated, {
  useAnimatedStyle,
  useSharedValue,
  withSpring,
} from "react-native-reanimated";
import * as Haptics from "expo-haptics";
import Colors from "@/constants/colors";
import { useDocuments, Document } from "@/context/DocumentsContext";

const { width } = Dimensions.get("window");
const CARD_WIDTH = width * 0.60;

function DocTypeIcon({ type, size = 20 }: { type: string; size?: number }) {
  const color =
    type === "pdf" ? "#EF4444" : type === "image" ? "#8B5CF6" : type === "word" ? "#2563EB" : "#10B981";
  const icon =
    type === "pdf" ? "file-pdf-box" : type === "image" ? "file-image" : type === "word" ? "file-word" : "file-excel";
  return <MaterialCommunityIcons name={icon as any} size={size} color={color} />;
}

function QuickActionItem({
  icon,
  label,
  color,
  onPress,
}: {
  icon: string;
  label: string;
  color: string;
  onPress: () => void;
}) {
  const scale = useSharedValue(1);
  const animStyle = useAnimatedStyle(() => ({ transform: [{ scale: scale.value }] }));
  const colorScheme = useColorScheme();
  const theme = colorScheme === "dark" ? Colors.dark : Colors.light;

  return (
    <Pressable
      onPressIn={() => { scale.value = withSpring(0.91); Haptics.impactAsync(Haptics.ImpactFeedbackStyle.Light); }}
      onPressOut={() => { scale.value = withSpring(1); }}
      onPress={onPress}
    >
      <Animated.View style={[styles.quickActionItem, animStyle]}>
        <View style={[styles.quickActionIcon, { backgroundColor: color + "1A" }]}>
          <Ionicons name={icon as any} size={24} color={color} />
        </View>
        <Text style={[styles.quickActionLabel, { color: theme.textSecondary, fontFamily: "Inter_500Medium" }]}>
          {label}
        </Text>
      </Animated.View>
    </Pressable>
  );
}

function AdBanner({
  title,
  subtitle,
  ctaLabel,
  accentColor,
  onDismiss,
  onPress,
}: {
  title: string;
  subtitle: string;
  ctaLabel: string;
  accentColor: string;
  onDismiss: () => void;
  onPress: () => void;
}) {
  const colorScheme = useColorScheme();
  const theme = colorScheme === "dark" ? Colors.dark : Colors.light;

  return (
    <View style={[styles.adBanner, { backgroundColor: theme.card, borderColor: theme.border }]}>
      <View style={[styles.adBadge, { backgroundColor: accentColor + "18" }]}>
        <Text style={[styles.adBadgeText, { color: accentColor, fontFamily: "Inter_600SemiBold" }]}>
          Sponsored
        </Text>
      </View>
      <View style={styles.adContent}>
        <View style={[styles.adIconWrap, { backgroundColor: accentColor + "15" }]}>
          <Ionicons name="sparkles" size={20} color={accentColor} />
        </View>
        <View style={styles.adText}>
          <Text style={[styles.adTitle, { color: theme.text, fontFamily: "Inter_600SemiBold" }]}>
            {title}
          </Text>
          <Text style={[styles.adSubtitle, { color: theme.textSecondary, fontFamily: "Inter_400Regular" }]}>
            {subtitle}
          </Text>
        </View>
        <Pressable style={[styles.adCta, { backgroundColor: accentColor }]} onPress={onPress}>
          <Text style={[styles.adCtaText, { fontFamily: "Inter_600SemiBold" }]}>{ctaLabel}</Text>
        </Pressable>
      </View>
      <Pressable style={styles.adClose} onPress={onDismiss}>
        <Ionicons name="close" size={14} color={theme.textMuted} />
      </Pressable>
    </View>
  );
}

function RecentDocCard({ doc }: { doc: Document }) {
  const colorScheme = useColorScheme();
  const theme = colorScheme === "dark" ? Colors.dark : Colors.light;
  const scale = useSharedValue(1);
  const animStyle = useAnimatedStyle(() => ({ transform: [{ scale: scale.value }] }));

  const formatDate = (ts: number) => {
    const diff = Math.floor((Date.now() - ts) / 86400000);
    if (diff === 0) return "Today";
    if (diff === 1) return "Yesterday";
    return `${diff}d ago`;
  };

  return (
    <Pressable
      onPressIn={() => { scale.value = withSpring(0.96); Haptics.impactAsync(Haptics.ImpactFeedbackStyle.Light); }}
      onPressOut={() => { scale.value = withSpring(1); }}
      onPress={() => router.push({ pathname: "/document/[id]", params: { id: doc.id } })}
    >
      <Animated.View style={[styles.recentCard, { backgroundColor: theme.card, width: CARD_WIDTH }, animStyle]}>
        <View style={[styles.recentCardPreview, { backgroundColor: theme.surfaceSecondary }]}>
          <DocTypeIcon type={doc.type} size={38} />
          {doc.starred && (
            <View style={styles.recentStarBadge}>
              <Ionicons name="star" size={12} color="#F59E0B" />
            </View>
          )}
        </View>
        <View style={styles.recentCardInfo}>
          <Text style={[styles.recentCardName, { color: theme.text, fontFamily: "Inter_600SemiBold" }]} numberOfLines={1}>
            {doc.name}
          </Text>
          <View style={styles.recentCardMeta}>
            <Text style={[styles.recentCardSize, { color: theme.textMuted, fontFamily: "Inter_400Regular" }]}>
              {doc.pages}p · {doc.size}
            </Text>
            <Text style={[styles.recentCardDate, { color: theme.textMuted, fontFamily: "Inter_400Regular" }]}>
              {formatDate(doc.updatedAt)}
            </Text>
          </View>
        </View>
      </Animated.View>
    </Pressable>
  );
}

function StatCard({ label, value, icon, color }: { label: string; value: string; icon: string; color: string }) {
  const colorScheme = useColorScheme();
  const theme = colorScheme === "dark" ? Colors.dark : Colors.light;
  return (
    <View style={[styles.statCard, { backgroundColor: theme.card }]}>
      <View style={[styles.statIconWrap, { backgroundColor: color + "18" }]}>
        <Ionicons name={icon as any} size={18} color={color} />
      </View>
      <Text style={[styles.statValue, { color: theme.text, fontFamily: "Inter_700Bold" }]}>{value}</Text>
      <Text style={[styles.statLabel, { color: theme.textMuted, fontFamily: "Inter_400Regular" }]}>{label}</Text>
    </View>
  );
}

function RecentListRow({ doc }: { doc: Document }) {
  const colorScheme = useColorScheme();
  const theme = colorScheme === "dark" ? Colors.dark : Colors.light;
  const formatDate = (ts: number) => {
    const diff = Math.floor((Date.now() - ts) / 86400000);
    if (diff === 0) return "Today";
    if (diff === 1) return "Yesterday";
    return `${diff}d ago`;
  };

  return (
    <Pressable
      style={[styles.recentListRow, { backgroundColor: theme.card }]}
      onPress={() => router.push({ pathname: "/document/[id]", params: { id: doc.id } })}
    >
      <View style={[styles.recentListIcon, { backgroundColor: theme.surfaceSecondary }]}>
        <DocTypeIcon type={doc.type} size={24} />
      </View>
      <View style={styles.recentListInfo}>
        <Text style={[styles.recentListName, { color: theme.text, fontFamily: "Inter_500Medium" }]} numberOfLines={1}>
          {doc.name}
        </Text>
        <Text style={[styles.recentListMeta, { color: theme.textMuted, fontFamily: "Inter_400Regular" }]}>
          {doc.pages}p · {doc.size}
        </Text>
      </View>
      <Text style={[styles.recentListDate, { color: theme.textMuted, fontFamily: "Inter_400Regular" }]}>
        {formatDate(doc.updatedAt)}
      </Text>
      <Ionicons name="chevron-forward" size={16} color={theme.textMuted} />
    </Pressable>
  );
}

const QUICK_ACTIONS = [
  { icon: "scan-outline", label: "Scan", color: "#2563EB", path: "/(tabs)/scan" as const },
  { icon: "cloud-upload-outline", label: "Import", color: "#8B5CF6", path: "/(tabs)/scan" as const },
  { icon: "git-merge-outline", label: "Merge", color: "#10B981", path: "/(tabs)/tools" as const },
  { icon: "cut-outline", label: "Split", color: "#F59E0B", path: "/(tabs)/tools" as const },
  { icon: "contract-outline", label: "Compress", color: "#EF4444", path: "/(tabs)/tools" as const },
  { icon: "shield-outline", label: "Protect", color: "#EC4899", path: "/(tabs)/tools" as const },
  { icon: "text-outline", label: "OCR", color: "#06B6D4", path: "/(tabs)/tools" as const },
  { icon: "pencil-outline", label: "Sign", color: "#8B5CF6", path: "/(tabs)/tools" as const },
];

export default function HomeScreen() {
  const colorScheme = useColorScheme();
  const isDark = colorScheme === "dark";
  const theme = isDark ? Colors.dark : Colors.light;
  const insets = useSafeAreaInsets();
  const { documents, recentDocuments, starredDocuments, totalSize, folders } = useDocuments();
  const [showTopAd, setShowTopAd] = useState(true);
  const [showMidAd, setShowMidAd] = useState(true);

  const topPad = Platform.OS === "web" ? Math.max(insets.top, 67) : insets.top;

  const greeting = () => {
    const h = new Date().getHours();
    if (h < 12) return "Good morning";
    if (h < 17) return "Good afternoon";
    return "Good evening";
  };

  return (
    <View style={[styles.container, { backgroundColor: theme.background }]}>
      <ScrollView
        showsVerticalScrollIndicator={false}
        contentContainerStyle={{ paddingBottom: Platform.OS === "web" ? 34 : insets.bottom + 90 }}
      >
        {/* Header */}
        <View style={[styles.header, { paddingTop: topPad + 16 }]}>
          <View>
            <Text style={[styles.greeting, { color: theme.textSecondary, fontFamily: "Inter_400Regular" }]}>
              {greeting()}
            </Text>
            <Text style={[styles.appTitle, { color: theme.text, fontFamily: "Inter_700Bold" }]}>
              PdfPeaks
            </Text>
          </View>
          <View style={styles.headerRight}>
            <Pressable
              style={[styles.headerBtn, { backgroundColor: theme.surfaceSecondary }]}
              onPress={() => router.push("/search")}
            >
              <Ionicons name="search-outline" size={20} color={theme.text} />
            </Pressable>
            <Pressable
              style={[styles.avatarBtn, { backgroundColor: theme.primary }]}
              onPress={() => router.push("/settings")}
            >
              <Text style={[styles.avatarText, { fontFamily: "Inter_700Bold" }]}>P</Text>
            </Pressable>
          </View>
        </View>

        {/* Stats Row */}
        <View style={styles.statsRow}>
          <StatCard label="Files" value={`${documents.length}`} icon="document-text" color="#2563EB" />
          <StatCard label="Starred" value={`${starredDocuments.length}`} icon="star" color="#F59E0B" />
          <StatCard label="Folders" value={`${folders.length}`} icon="folder" color="#8B5CF6" />
          <StatCard label="Storage" value={totalSize} icon="server" color="#10B981" />
        </View>

        {/* Non-intrusive top Ad — between stats and quick actions */}
        {showTopAd && (
          <View style={styles.adWrap}>
            <AdBanner
              title="Upgrade to Pro"
              subtitle="Unlimited scans, OCR & cloud sync"
              ctaLabel="Try Free"
              accentColor="#2563EB"
              onDismiss={() => setShowTopAd(false)}
              onPress={() => router.push("/settings")}
            />
          </View>
        )}

        {/* Quick Actions — horizontal scroll */}
        <View style={styles.sectionHeader}>
          <Text style={[styles.sectionTitle, { color: theme.text, fontFamily: "Inter_600SemiBold" }]}>
            Quick Actions
          </Text>
        </View>
        <ScrollView
          horizontal
          showsHorizontalScrollIndicator={false}
          contentContainerStyle={styles.quickActionsScroll}
        >
          {QUICK_ACTIONS.map((a) => (
            <QuickActionItem
              key={a.label}
              icon={a.icon}
              label={a.label}
              color={a.color}
              onPress={() => router.push(a.path)}
            />
          ))}
        </ScrollView>

        {/* Recent Documents — horizontal cards */}
        <View style={[styles.sectionHeader, { marginTop: 24 }]}>
          <Text style={[styles.sectionTitle, { color: theme.text, fontFamily: "Inter_600SemiBold" }]}>
            Recent Files
          </Text>
          <Pressable onPress={() => router.push("/(tabs)/documents")}>
            <Text style={[styles.seeAll, { color: theme.primary, fontFamily: "Inter_500Medium" }]}>
              See all
            </Text>
          </Pressable>
        </View>

        {recentDocuments.length === 0 ? (
          <View style={styles.emptyState}>
            <Ionicons name="document-outline" size={40} color={theme.textMuted} />
            <Text style={[styles.emptyText, { color: theme.textMuted, fontFamily: "Inter_400Regular" }]}>
              No documents yet. Scan or import to get started.
            </Text>
          </View>
        ) : (
          <ScrollView
            horizontal
            showsHorizontalScrollIndicator={false}
            contentContainerStyle={styles.recentScroll}
          >
            {recentDocuments.map((doc) => (
              <RecentDocCard key={doc.id} doc={doc} />
            ))}
          </ScrollView>
        )}

        {/* Recent files as list rows */}
        {recentDocuments.length > 0 && (
          <View style={[styles.recentList, { marginTop: 16 }]}>
            <View style={[styles.sectionHeader, { paddingHorizontal: 20 }]}>
              <Text style={[styles.sectionTitle, { color: theme.text, fontFamily: "Inter_600SemiBold" }]}>
                All Recent
              </Text>
            </View>
            <View style={styles.listContainer}>
              {recentDocuments.map((doc, index) => (
                <React.Fragment key={doc.id}>
                  <RecentListRow doc={doc} />
                  {/* Mid Ad — placed naturally after 2nd item, non-blocking */}
                  {index === 1 && showMidAd && (
                    <AdBanner
                      title="CamScanner Alternative"
                      subtitle="Better quality, more features, free"
                      ctaLabel="Learn More"
                      accentColor="#10B981"
                      onDismiss={() => setShowMidAd(false)}
                      onPress={() => {}}
                    />
                  )}
                </React.Fragment>
              ))}
            </View>
          </View>
        )}

        {/* Starred section */}
        {starredDocuments.length > 0 && (
          <View style={{ marginTop: 24 }}>
            <View style={[styles.sectionHeader, { paddingHorizontal: 20 }]}>
              <Text style={[styles.sectionTitle, { color: theme.text, fontFamily: "Inter_600SemiBold" }]}>
                Starred
              </Text>
            </View>
            <View style={styles.listContainer}>
              {starredDocuments.slice(0, 3).map((doc) => (
                <RecentListRow key={doc.id} doc={doc} />
              ))}
            </View>
          </View>
        )}
      </ScrollView>
    </View>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  header: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    paddingHorizontal: 20,
    paddingBottom: 20,
  },
  greeting: { fontSize: 13, marginBottom: 2 },
  appTitle: { fontSize: 28 },
  headerRight: { flexDirection: "row", alignItems: "center", gap: 10 },
  headerBtn: { width: 38, height: 38, borderRadius: 19, alignItems: "center", justifyContent: "center" },
  avatarBtn: { width: 38, height: 38, borderRadius: 19, alignItems: "center", justifyContent: "center" },
  avatarText: { color: "#fff", fontSize: 15 },
  statsRow: {
    flexDirection: "row",
    paddingHorizontal: 16,
    gap: 10,
    marginBottom: 0,
  },
  statCard: { flex: 1, borderRadius: 14, padding: 12, alignItems: "center", gap: 4 },
  statIconWrap: { width: 34, height: 34, borderRadius: 10, alignItems: "center", justifyContent: "center", marginBottom: 2 },
  statValue: { fontSize: 15 },
  statLabel: { fontSize: 10, textAlign: "center" },

  adWrap: { paddingHorizontal: 16, marginTop: 16 },
  adBanner: {
    borderRadius: 16,
    borderWidth: 1,
    padding: 14,
    position: "relative",
    overflow: "hidden",
  },
  adBadge: { alignSelf: "flex-start", paddingHorizontal: 8, paddingVertical: 3, borderRadius: 6, marginBottom: 10 },
  adBadgeText: { fontSize: 10 },
  adContent: { flexDirection: "row", alignItems: "center", gap: 12 },
  adIconWrap: { width: 40, height: 40, borderRadius: 12, alignItems: "center", justifyContent: "center" },
  adText: { flex: 1 },
  adTitle: { fontSize: 14, marginBottom: 2 },
  adSubtitle: { fontSize: 12 },
  adCta: { paddingHorizontal: 12, paddingVertical: 8, borderRadius: 10 },
  adCtaText: { color: "#fff", fontSize: 12 },
  adClose: { position: "absolute", top: 10, right: 10, padding: 4 },

  sectionHeader: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    paddingHorizontal: 20,
  },
  sectionTitle: { fontSize: 18, marginBottom: 12 },
  seeAll: { fontSize: 14, marginBottom: 12 },

  quickActionsScroll: { paddingHorizontal: 16, gap: 10 },
  quickActionItem: { alignItems: "center", gap: 8, width: 72 },
  quickActionIcon: { width: 56, height: 56, borderRadius: 18, alignItems: "center", justifyContent: "center" },
  quickActionLabel: { fontSize: 11, textAlign: "center" },

  recentScroll: { paddingLeft: 20, paddingRight: 8, gap: 12 },
  recentCard: {
    borderRadius: 16,
    overflow: "hidden",
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.07,
    shadowRadius: 8,
    elevation: 3,
  },
  recentCardPreview: {
    height: 110,
    alignItems: "center",
    justifyContent: "center",
    position: "relative",
  },
  recentStarBadge: {
    position: "absolute",
    top: 8,
    right: 8,
    backgroundColor: "rgba(0,0,0,0.15)",
    borderRadius: 8,
    padding: 4,
  },
  recentCardInfo: { padding: 12 },
  recentCardName: { fontSize: 13, marginBottom: 6 },
  recentCardMeta: { flexDirection: "row", justifyContent: "space-between" },
  recentCardSize: { fontSize: 11 },
  recentCardDate: { fontSize: 11 },

  recentList: {},
  listContainer: { paddingHorizontal: 16, gap: 8 },
  recentListRow: {
    flexDirection: "row",
    alignItems: "center",
    borderRadius: 14,
    padding: 14,
    gap: 12,
  },
  recentListIcon: { width: 44, height: 44, borderRadius: 12, alignItems: "center", justifyContent: "center" },
  recentListInfo: { flex: 1 },
  recentListName: { fontSize: 14, marginBottom: 3 },
  recentListMeta: { fontSize: 12 },
  recentListDate: { fontSize: 12, marginRight: 4 },

  emptyState: { alignItems: "center", paddingVertical: 28, gap: 10, paddingHorizontal: 40 },
  emptyText: { fontSize: 14, textAlign: "center" },
});
