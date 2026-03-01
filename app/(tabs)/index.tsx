import React, { useRef } from "react";
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

const CARD_WIDTH = width * 0.62;

function DocTypeIcon({ type, size = 20 }: { type: string; size?: number }) {
  const color =
    type === "pdf"
      ? "#EF4444"
      : type === "image"
      ? "#8B5CF6"
      : type === "word"
      ? "#2563EB"
      : "#10B981";
  const icon =
    type === "pdf"
      ? "file-pdf-box"
      : type === "image"
      ? "file-image"
      : type === "word"
      ? "file-word"
      : "file-excel";
  return <MaterialCommunityIcons name={icon as any} size={size} color={color} />;
}

function QuickAction({
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
  const style = useAnimatedStyle(() => ({ transform: [{ scale: scale.value }] }));
  const colorScheme = useColorScheme();
  const theme = colorScheme === "dark" ? Colors.dark : Colors.light;

  return (
    <Pressable
      onPressIn={() => {
        scale.value = withSpring(0.93);
        Haptics.impactAsync(Haptics.ImpactFeedbackStyle.Light);
      }}
      onPressOut={() => { scale.value = withSpring(1); }}
      onPress={onPress}
    >
      <Animated.View style={[styles.quickAction, style]}>
        <View style={[styles.quickActionIcon, { backgroundColor: color + "20" }]}>
          <Ionicons name={icon as any} size={22} color={color} />
        </View>
        <Text style={[styles.quickActionLabel, { color: theme.textSecondary, fontFamily: "Inter_500Medium" }]}>
          {label}
        </Text>
      </Animated.View>
    </Pressable>
  );
}

function RecentDocCard({ doc }: { doc: Document }) {
  const colorScheme = useColorScheme();
  const theme = colorScheme === "dark" ? Colors.dark : Colors.light;
  const scale = useSharedValue(1);
  const style = useAnimatedStyle(() => ({ transform: [{ scale: scale.value }] }));

  const formatDate = (ts: number) => {
    const d = new Date(ts);
    const now = new Date();
    const diff = Math.floor((now.getTime() - d.getTime()) / 86400000);
    if (diff === 0) return "Today";
    if (diff === 1) return "Yesterday";
    return `${diff} days ago`;
  };

  return (
    <Pressable
      onPressIn={() => { scale.value = withSpring(0.96); Haptics.impactAsync(Haptics.ImpactFeedbackStyle.Light); }}
      onPressOut={() => { scale.value = withSpring(1); }}
      onPress={() => router.push({ pathname: "/document/[id]", params: { id: doc.id } })}
    >
      <Animated.View style={[styles.recentCard, { backgroundColor: theme.card, width: CARD_WIDTH }, style]}>
        <View style={[styles.recentCardPreview, { backgroundColor: theme.surfaceSecondary }]}>
          <DocTypeIcon type={doc.type} size={36} />
        </View>
        <View style={styles.recentCardInfo}>
          <Text style={[styles.recentCardName, { color: theme.text, fontFamily: "Inter_600SemiBold" }]} numberOfLines={1}>
            {doc.name}
          </Text>
          <View style={styles.recentCardMeta}>
            <Text style={[styles.recentCardSize, { color: theme.textMuted, fontFamily: "Inter_400Regular" }]}>
              {doc.pages}p · {doc.size}
            </Text>
          </View>
          <Text style={[styles.recentCardDate, { color: theme.textMuted, fontFamily: "Inter_400Regular" }]}>
            {formatDate(doc.updatedAt)}
          </Text>
        </View>
        {doc.starred && (
          <Ionicons name="star" size={14} color="#F59E0B" style={styles.starBadge} />
        )}
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
        <Ionicons name={icon as any} size={20} color={color} />
      </View>
      <Text style={[styles.statValue, { color: theme.text, fontFamily: "Inter_700Bold" }]}>{value}</Text>
      <Text style={[styles.statLabel, { color: theme.textMuted, fontFamily: "Inter_400Regular" }]}>{label}</Text>
    </View>
  );
}

export default function HomeScreen() {
  const colorScheme = useColorScheme();
  const isDark = colorScheme === "dark";
  const theme = isDark ? Colors.dark : Colors.light;
  const insets = useSafeAreaInsets();
  const { documents, recentDocuments, starredDocuments, totalSize, folders } = useDocuments();

  const topPad = Platform.OS === "web" ? Math.max(insets.top, 67) : insets.top;

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
              Good morning
            </Text>
            <Text style={[styles.appTitle, { color: theme.text, fontFamily: "Inter_700Bold" }]}>
              PdfPeaks
            </Text>
          </View>
          <Pressable
            style={[styles.avatarBtn, { backgroundColor: theme.primary }]}
            onPress={() => Haptics.impactAsync(Haptics.ImpactFeedbackStyle.Light)}
          >
            <Text style={[styles.avatarText, { fontFamily: "Inter_700Bold" }]}>P</Text>
          </Pressable>
        </View>

        {/* Stats Row */}
        <View style={styles.statsRow}>
          <StatCard label="Documents" value={`${documents.length}`} icon="document-text" color="#2563EB" />
          <StatCard label="Starred" value={`${starredDocuments.length}`} icon="star" color="#F59E0B" />
          <StatCard label="Folders" value={`${folders.length}`} icon="folder" color="#8B5CF6" />
          <StatCard label="Total Size" value={totalSize} icon="server" color="#10B981" />
        </View>

        {/* Quick Actions */}
        <View style={styles.section}>
          <Text style={[styles.sectionTitle, { color: theme.text, fontFamily: "Inter_600SemiBold" }]}>
            Quick Actions
          </Text>
          <View style={styles.quickActionsGrid}>
            <QuickAction icon="scan-outline" label="Scan" color="#2563EB" onPress={() => router.push("/(tabs)/scan")} />
            <QuickAction icon="cloud-upload-outline" label="Import" color="#8B5CF6" onPress={() => router.push("/(tabs)/scan")} />
            <QuickAction icon="folder-outline" label="New Folder" color="#F59E0B" onPress={() => router.push("/(tabs)/documents")} />
            <QuickAction icon="construct-outline" label="Tools" color="#10B981" onPress={() => router.push("/(tabs)/tools")} />
          </View>
        </View>

        {/* Recent Documents */}
        <View style={styles.section}>
          <View style={styles.sectionHeader}>
            <Text style={[styles.sectionTitle, { color: theme.text, fontFamily: "Inter_600SemiBold" }]}>
              Recent
            </Text>
            <Pressable onPress={() => router.push("/(tabs)/documents")}>
              <Text style={[styles.seeAll, { color: theme.primary, fontFamily: "Inter_500Medium" }]}>
                See all
              </Text>
            </Pressable>
          </View>
        </View>

        {recentDocuments.length === 0 ? (
          <View style={styles.emptyRecent}>
            <Ionicons name="document-outline" size={40} color={theme.textMuted} />
            <Text style={[styles.emptyText, { color: theme.textMuted, fontFamily: "Inter_400Regular" }]}>
              No documents yet. Start by scanning or importing.
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

        {/* Starred */}
        {starredDocuments.length > 0 && (
          <View style={styles.section}>
            <Text style={[styles.sectionTitle, { color: theme.text, fontFamily: "Inter_600SemiBold" }]}>
              Starred
            </Text>
            {starredDocuments.map((doc) => (
              <Pressable
                key={doc.id}
                style={[styles.listItem, { backgroundColor: theme.card }]}
                onPress={() => router.push({ pathname: "/document/[id]", params: { id: doc.id } })}
              >
                <View style={[styles.listItemIcon, { backgroundColor: theme.surfaceSecondary }]}>
                  <DocTypeIcon type={doc.type} size={24} />
                </View>
                <View style={styles.listItemInfo}>
                  <Text style={[styles.listItemName, { color: theme.text, fontFamily: "Inter_500Medium" }]} numberOfLines={1}>
                    {doc.name}
                  </Text>
                  <Text style={[styles.listItemMeta, { color: theme.textMuted, fontFamily: "Inter_400Regular" }]}>
                    {doc.pages} pages · {doc.size}
                  </Text>
                </View>
                <Ionicons name="star" size={14} color="#F59E0B" />
              </Pressable>
            ))}
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
  avatarBtn: {
    width: 40,
    height: 40,
    borderRadius: 20,
    alignItems: "center",
    justifyContent: "center",
  },
  avatarText: { color: "#fff", fontSize: 16 },
  statsRow: {
    flexDirection: "row",
    paddingHorizontal: 16,
    gap: 10,
    marginBottom: 8,
  },
  statCard: {
    flex: 1,
    borderRadius: 14,
    padding: 12,
    alignItems: "center",
    gap: 4,
  },
  statIconWrap: {
    width: 36,
    height: 36,
    borderRadius: 10,
    alignItems: "center",
    justifyContent: "center",
    marginBottom: 4,
  },
  statValue: { fontSize: 16 },
  statLabel: { fontSize: 10, textAlign: "center" },
  section: { paddingHorizontal: 20, marginTop: 24 },
  sectionHeader: { flexDirection: "row", justifyContent: "space-between", alignItems: "center" },
  sectionTitle: { fontSize: 18, marginBottom: 12 },
  seeAll: { fontSize: 14 },
  quickActionsGrid: { flexDirection: "row", gap: 12, flexWrap: "wrap" },
  quickAction: { alignItems: "center", gap: 8, width: (width - 40 - 36) / 4 },
  quickActionIcon: {
    width: 52,
    height: 52,
    borderRadius: 16,
    alignItems: "center",
    justifyContent: "center",
  },
  quickActionLabel: { fontSize: 11, textAlign: "center" },
  recentScroll: { paddingLeft: 20, paddingRight: 12, gap: 12 },
  recentCard: {
    borderRadius: 16,
    overflow: "hidden",
    shadowColor: "#000",
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.08,
    shadowRadius: 8,
    elevation: 3,
  },
  recentCardPreview: {
    height: 120,
    alignItems: "center",
    justifyContent: "center",
  },
  recentCardInfo: { padding: 12 },
  recentCardName: { fontSize: 14, marginBottom: 4 },
  recentCardMeta: { flexDirection: "row" },
  recentCardSize: { fontSize: 12 },
  recentCardDate: { fontSize: 11, marginTop: 4 },
  starBadge: { position: "absolute", top: 10, right: 10 },
  emptyRecent: { alignItems: "center", paddingVertical: 30, gap: 10, paddingHorizontal: 40 },
  emptyText: { fontSize: 14, textAlign: "center" },
  listItem: {
    flexDirection: "row",
    alignItems: "center",
    borderRadius: 14,
    padding: 14,
    marginBottom: 8,
    gap: 12,
  },
  listItemIcon: {
    width: 44,
    height: 44,
    borderRadius: 12,
    alignItems: "center",
    justifyContent: "center",
  },
  listItemInfo: { flex: 1 },
  listItemName: { fontSize: 14, marginBottom: 3 },
  listItemMeta: { fontSize: 12 },
});
